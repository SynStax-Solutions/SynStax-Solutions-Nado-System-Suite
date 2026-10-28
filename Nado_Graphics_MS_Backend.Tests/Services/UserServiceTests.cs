using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Security;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IEmailService> _email = new();
    //-------------------------------------------------------------------------------
    private UserService Sut
    {
        get
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            return new UserService(_users.Object, _email.Object, config);
        }
    }
    //-------------------------------------------------------------------------------
    private void EmailSendsOk() =>
        _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
              .ReturnsAsync(true);
    //-------------------------------------------------------------------------------
    //CreateUser

    [Fact]
    public async Task Create_InvalidRole_Fails()
    {
        var result = await Sut.CreateUserAsync(
            new UserRequest { UserRole = "Wizard", Email = "a@b.com" }, AppRoles.Manager);

        result.Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_DuplicateEmail_Fails()
    {
        _users.Setup(u => u.EmailExistsAsync("a@b.com", null)).ReturnsAsync(true);

        var result = await Sut.CreateUserAsync(new UserRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = "A@B.com",
            Password = "Passw0rd!",
            UserRole = AppRoles.Designer
        }, AppRoles.Manager);

        result.Success.Should().BeFalse();
        _users.Verify(u => u.AddAsync(It.IsAny<User>()), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_Valid_NormalisesEmail_HashesPassword_AndSendsVerification()
    {
        EmailSendsOk();
        User? saved = null;
        _users.Setup(u => u.AddAsync(It.IsAny<User>())).Callback<User>(u => saved = u).Returns(Task.CompletedTask);

        var result = await Sut.CreateUserAsync(new UserRequest
        {
            FirstName = " Sam ",
            LastName = "Lee",
            Email = "  Sam@Nado.COM ",
            Password = "Passw0rd!",
            UserRole = AppRoles.Designer
        }, AppRoles.Manager);

        result.Success.Should().BeTrue();
        saved!.Email.Should().Be("sam@nado.com");
        saved.FirstName.Should().Be("Sam");
        saved.PasswordHash.Should().NotBe("Passw0rd!");
        BCrypt.Net.BCrypt.Verify("Passw0rd!", saved.PasswordHash).Should().BeTrue();
        saved.VerificationTokenHash.Should().NotBeNullOrEmpty();
        _email.Verify(e => e.SendAsync("sam@nado.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Create_WhenEmailFails_StillSucceeds_WithWarningMessage()
    {
        _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
              .ReturnsAsync(false);

        var result = await Sut.CreateUserAsync(new UserRequest
        {
            FirstName = "A",
            LastName = "B",
            Email = "a@b.com",
            Password = "Passw0rd!",
            UserRole = AppRoles.Designer
        }, AppRoles.Manager);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("could not be sent");
    }

    //-------------------------------------------------------------------------------
    //UpdateUser / DeleteUser

    [Fact]
    public async Task Update_UserNotFound_Fails()
    {
        _users.Setup(u => u.GetByIdAsync(5)).ReturnsAsync((User?)null);

        (await Sut.UpdateUserAsync(5, new UpdateUser(), 1, AppRoles.Manager)).Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Update_OwnRoleChange_Fails()
    {
        _users.Setup(u => u.GetByIdAsync(1))
              .ReturnsAsync(new User { UserID = 1, UserRole = AppRoles.Manager, Email = "m@n.com" });

        var result = await Sut.UpdateUserAsync(1, new UpdateUser
        {
            FirstName = "M",
            LastName = "N",
            Email = "m@n.com",
            UserRole = AppRoles.Designer,
            IsActive = true
        }, 1, AppRoles.Manager);

        result.Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_OwnAccount_Fails()
    {
        _users.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(new User { UserID = 1, UserRole = AppRoles.Manager });

        (await Sut.DeleteUserAsync(1, 1, AppRoles.Manager)).Success.Should().BeFalse();
        _users.Verify(u => u.Remove(It.IsAny<User>()), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Delete_LinkedToOtherRecords_ReturnsFriendlyMessage()
    {
        _users.Setup(u => u.GetByIdAsync(2)).ReturnsAsync(new User { UserID = 2, UserRole = AppRoles.Designer });
        _users.Setup(u => u.SaveChangesAsync()).ThrowsAsync(new DbUpdateException());

        var result = await Sut.DeleteUserAsync(2, 1, AppRoles.Manager);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Deactivate");
    }
    //-------------------------------------------------------------------------------
    // Passwords

    [Fact]
    public async Task ChangePassword_WrongCurrent_Fails()
    {
        var user = new User { UserID = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Correct1!") };
        _users.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await Sut.ChangePasswordAsync(1,
            new ChangePasswordRequest { CurrentPassword = "Wrong1!", NewPassword = "NewPass1!" });

        result.Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ChangePassword_SameAsCurrent_Fails()
    {
        var user = new User { UserID = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Same1234!") };
        _users.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await Sut.ChangePasswordAsync(1,
            new ChangePasswordRequest { CurrentPassword = "Same1234!", NewPassword = "Same1234!" });

        result.Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ChangePassword_Valid_UpdatesHashAndTimestamp()
    {
        var user = new User { UserID = 1, PasswordHash = BCrypt.Net.BCrypt.HashPassword("Old12345!") };
        _users.Setup(u => u.GetByIdAsync(1)).ReturnsAsync(user);

        var result = await Sut.ChangePasswordAsync(1,
            new ChangePasswordRequest { CurrentPassword = "Old12345!", NewPassword = "New12345!" });

        result.Success.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("New12345!", user.PasswordHash).Should().BeTrue();
        user.PasswordChangedAt.Should().NotBeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ForgotPassword_UnknownEmail_ReturnsSameMessage_AndSendsNothing()
    {
        _users.Setup(u => u.GetByEmailAsync("nobody@x.com")).ReturnsAsync((User?)null);
        EmailSendsOk();

        var unknown = await Sut.ForgotPasswordAsync("nobody@x.com");

        unknown.Success.Should().BeTrue();
        _email.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ForgotPassword_KnownUser_StoresHashedToken_AndEmails_WithSameMessage()
    {
        EmailSendsOk();
        var user = new User { UserID = 1, Email = "u@x.com", FirstName = "U", IsActive = true };
        _users.Setup(u => u.GetByEmailAsync("u@x.com")).ReturnsAsync(user);
        _users.Setup(u => u.GetByEmailAsync("nobody@x.com")).ReturnsAsync((User?)null);

        var known = await Sut.ForgotPasswordAsync("u@x.com");
        var unknown = await Sut.ForgotPasswordAsync("nobody@x.com");

        user.ResetTokenHash.Should().NotBeNullOrEmpty();
        user.ResetTokenExpiry.Should().BeAfter(DateTime.UtcNow);
        known.Message.Should().Be(unknown.Message);   //no way to tell which emails exist
        _email.Verify(e => e.SendAsync("u@x.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ResetPassword_UnknownToken_Fails()
    {
        _users.Setup(u => u.GetByResetTokenHashAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        (await Sut.ResetPasswordAsync("abc", "NewPass1!")).Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ResetPassword_ExpiredToken_Fails()
    {
        var user = new User { ResetTokenExpiry = DateTime.UtcNow.AddMinutes(-1) };
        _users.Setup(u => u.GetByResetTokenHashAsync(It.IsAny<string>())).ReturnsAsync(user);

        (await Sut.ResetPasswordAsync("abc", "NewPass1!")).Success.Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task ResetPassword_ValidToken_ChangesPassword_ClearsToken_AndLockout()
    {
        var user = new User
        {
            ResetTokenHash = "x",
            ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10),
            FailedLoginAttempts = 5,
            LockoutEnd = DateTime.UtcNow.AddMinutes(15)
        };
        _users.Setup(u => u.GetByResetTokenHashAsync(It.IsAny<string>())).ReturnsAsync(user);

        var result = await Sut.ResetPasswordAsync("abc", "NewPass1!");

        result.Success.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("NewPass1!", user.PasswordHash).Should().BeTrue();
        user.ResetTokenHash.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    //Email verification

    [Fact]
    public async Task VerifyEmail_ValidToken_MarksVerified()
    {
        var user = new User { VerificationTokenExpiry = DateTime.UtcNow.AddHours(1), VerificationTokenHash = "x" };
        _users.Setup(u => u.GetByVerificationTokenHashAsync(It.IsAny<string>())).ReturnsAsync(user);

        var result = await Sut.VerifyEmailAsync("abc");

        result.Success.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        user.VerificationTokenHash.Should().BeNull();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task VerifyEmail_ExpiredToken_Fails()
    {
        var user = new User { VerificationTokenExpiry = DateTime.UtcNow.AddHours(-1) };
        _users.Setup(u => u.GetByVerificationTokenHashAsync(It.IsAny<string>())).ReturnsAsync(user);

        (await Sut.VerifyEmailAsync("abc")).Success.Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------