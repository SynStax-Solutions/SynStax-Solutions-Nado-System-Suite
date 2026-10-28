using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();

    //-------------------------------------------------------------------------------
    private AuthService Sut
    {
        get
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-test-signing-key-with-32-plus-chars!",
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test"
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new AuthService(_users.Object, new TokenService(config));
        }
    }
    //-------------------------------------------------------------------------------
    private User GoodUser(string password = "Passw0rd!")
    {
        var user = new User
        {
            UserID = 1,
            FirstName = "Ann",
            LastName = "Lee",
            Email = "ann@nado.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            UserRole = "Manager",
            IsActive = true,
            EmailVerified = true
        };
        _users.Setup(u => u.GetByEmailAsync("ann@nado.com")).ReturnsAsync(user);
        return user;
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_UnknownEmail_Returns401_WithGenericMessage()
    {
        _users.Setup(u => u.GetByEmailAsync("nobody@x.com")).ReturnsAsync((User?)null);

        var (response, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "nobody@x.com", Password = "whatever" });

        response.Should().BeNull();
        status.Should().Be(401);
        error.Should().Be("Invalid email or password.");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_WrongPassword_Returns401_AndCountsFailedAttempt()
    {
        var user = GoodUser();

        var (_, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Wrong1!" });

        status.Should().Be(401);
        error.Should().Be("Invalid email or password.");   //same message as unknown email
        user.FailedLoginAttempts.Should().Be(1);
        _users.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_FifthWrongPassword_LocksAccount_With403()
    {
        var user = GoodUser();
        user.FailedLoginAttempts = 4;

        var (_, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Wrong1!" });

        status.Should().Be(403);
        error.Should().Contain("Too many failed login attempts");
        user.LockoutEnd.Should().BeAfter(DateTime.UtcNow);
        user.FailedLoginAttempts.Should().Be(0);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_LockedAccount_IsRefused_EvenWithCorrectPassword()
    {
        var user = GoodUser();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);

        var (response, status, _) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Passw0rd!" });

        response.Should().BeNull();
        status.Should().Be(403);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_Valid_Returns200_WithToken_AndClearsFailedAttempts()
    {
        var user = GoodUser();
        user.FailedLoginAttempts = 3;

        var (response, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Passw0rd!" });

        status.Should().Be(200);
        error.Should().BeNull();
        response!.Token.Should().NotBeNullOrEmpty();
        response.UserRole.Should().Be("Manager");
        user.FailedLoginAttempts.Should().Be(0);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_EmailIsTrimmedAndLowercased()
    {
        GoodUser();

        var (_, status, _) = await Sut.LoginAsync(
            new LoginRequest { Email = "  ANN@Nado.com ", Password = "Passw0rd!" });

        status.Should().Be(200);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_UnverifiedEmail_Returns403()
    {
        var user = GoodUser();
        user.EmailVerified = false;

        var (response, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Passw0rd!" });

        response.Should().BeNull();
        status.Should().Be(403);
        error.Should().Contain("verify your email");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Login_DeactivatedAccount_Returns403()
    {
        var user = GoodUser();
        user.IsActive = false;

        var (response, status, error) = await Sut.LoginAsync(
            new LoginRequest { Email = "ann@nado.com", Password = "Passw0rd!" });

        response.Should().BeNull();
        status.Should().Be(403);
        error.Should().Contain("deactivated");
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------