namespace Nado_Graphics_MS_Backend.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
    }
}