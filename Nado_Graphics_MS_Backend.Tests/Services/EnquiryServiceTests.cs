using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Nado_Graphics_MS_Backend.Models;
using Nado_Graphics_MS_Backend.Repositories;
using Nado_Graphics_MS_Backend.Services;
using Xunit;

public class EnquiryServiceTests
{
    private readonly Mock<IEnquiryRepository> _repo = new();
    private readonly Mock<IEmailService> _email = new();
    //-------------------------------------------------------------------------------
    private EnquiryService CreateSut(string? paEmail = "pa@nado.test")
    {
        var settings = new Dictionary<string, string?> { ["PaInbox:Email"] = paEmail };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new EnquiryService(_repo.Object, _email.Object, config);
    }
    //-------------------------------------------------------------------------------
    private static EnquiryRequest ValidRequest() => new()
    {
        Name = "Jane Doe",
        Email = "jane@example.com",
        Message = "I need a banner.",
        ConsentGiven = true
    };
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Honeypot_FilledIn_ReturnsTrue_ButSavesNothing()
    {
        var request = ValidRequest();
        request.Website = "http://spam.example";

        (await CreateSut().SubmitEnquiryAsync(request, "10.0.0.1")).Should().BeTrue();
        _repo.Verify(r => r.AddAsync(It.IsAny<Enquiry>()), Times.Never);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task MissingMessage_ReturnsFalse()
    {
        var request = ValidRequest();
        request.Message = "  ";

        (await CreateSut().SubmitEnquiryAsync(request, "10.0.0.2")).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task NoConsent_ReturnsFalse()
    {
        var request = ValidRequest();
        request.ConsentGiven = false;

        (await CreateSut().SubmitEnquiryAsync(request, "10.0.0.3")).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task InvalidEmail_ReturnsFalse()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        (await CreateSut().SubmitEnquiryAsync(request, "10.0.0.4")).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task Valid_SavesEnquiry_AndEmailsPa()
    {
        var ok = await CreateSut().SubmitEnquiryAsync(ValidRequest(), "10.0.0.5");

        ok.Should().BeTrue();
        _repo.Verify(r => r.AddAsync(It.Is<Enquiry>(e => e.Email == "jane@example.com" && !e.IsRead)), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _email.Verify(e => e.SendAsync("pa@nado.test", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task NameWithLineBreaks_IsCleaned()
    {
        var request = ValidRequest();
        request.Name = "Jane\r\nBcc: evil@example.com";
        Enquiry? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Enquiry>())).Callback<Enquiry>(e => saved = e).Returns(Task.CompletedTask);

        await CreateSut().SubmitEnquiryAsync(request, "10.0.0.6");

        saved!.Name.Should().NotContain("\r").And.NotContain("\n");
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task EmailFailure_StillReturnsTrue()
    {
        _email.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
              .ThrowsAsync(new InvalidOperationException("SMTP down"));

        (await CreateSut().SubmitEnquiryAsync(ValidRequest(), "10.0.0.7")).Should().BeTrue();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task FourthSubmissionFromSameIp_IsRateLimited()
    {
        var sut = CreateSut();

        (await sut.SubmitEnquiryAsync(ValidRequest(), "10.0.0.8")).Should().BeTrue();
        (await sut.SubmitEnquiryAsync(ValidRequest(), "10.0.0.8")).Should().BeTrue();
        (await sut.SubmitEnquiryAsync(ValidRequest(), "10.0.0.8")).Should().BeTrue();
        (await sut.SubmitEnquiryAsync(ValidRequest(), "10.0.0.8")).Should().BeFalse();
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task MarkAsRead_SetsFlagAndSaves()
    {
        var enquiry = new Enquiry { IsRead = false };
        _repo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(enquiry);

        (await CreateSut().MarkAsReadAsync(1)).Should().BeTrue();
        enquiry.IsRead.Should().BeTrue();
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
    //-------------------------------------------------------------------------------
    [Fact]
    public async Task MarkAsRead_Missing_ReturnsFalse()
    {
        _repo.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((Enquiry?)null);

        (await CreateSut().MarkAsReadAsync(9)).Should().BeFalse();
    }
}
//--------------------------------------------------------------END OF FILE---------------------------------------------------------------