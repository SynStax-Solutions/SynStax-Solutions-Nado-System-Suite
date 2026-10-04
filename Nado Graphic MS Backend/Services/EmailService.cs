using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Nado_Graphics_MS_Backend.Services
{
    //This class sends emails using MailKit and the settings in the "Email" section.
    //MailKit is used because Microsoft recommends it instead of the older SmtpClient.
    public class EmailService
    {
        //The below lines store the app settings and the logger.
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        //This tells us if the app is running in Development or Production.
        private readonly IWebHostEnvironment _environment;

        //The constructor receives the settings, logger and environment using dependency injection.
        public EmailService(IConfiguration config, ILogger<EmailService> logger, IWebHostEnvironment environment)
        {
            _config = config;
            _logger = logger;
            _environment = environment;
        }

        //----------------------------------------------------------------------------------//
        //This sends an email and returns true if it worked.
        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
        {
            //This gets the email server address from the settings.
            var host = _config["Email:SmtpHost"];

            //This runs when no email server is set up.
            if (string.IsNullOrWhiteSpace(host))
            {
                //In Development the email is shown in the backend console instead,
                //so we can test without a real email account.
                if (_environment.IsDevelopment())
                {
                    _logger.LogWarning(
                        "Email:SmtpHost is not set, so the email was NOT sent. It is shown here instead.\n" +
                        "To: {To}\nSubject: {Subject}\n{Body}", toEmail, subject, htmlBody);
                    return true;
                }

                //In Production the email body is NOT logged, because it contains
                //verification and reset links that would end up in the server logs.
                _logger.LogError("Email:SmtpHost is not set, so the email to {To} could not be sent.", toEmail);
                return false;
            }

            try
            {
                //This gets who the email is from.
                var fromAddress = _config["Email:FromAddress"] ?? _config["Email:Username"]!;
                var fromName = _config["Email:FromName"] ?? "Nado Graphics";

                //This builds the email.
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromName, fromAddress));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                //This works out the type of secure connection from the port.
                //Port 465 uses SSL straight away, port 587 upgrades to a secure connection (STARTTLS).
                var port = _config.GetValue("Email:SmtpPort", 587);
                var security = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

                //This connects to the email server, logs in and sends the email.
                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, security);
                await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                //This logs the error if the email could not be sent.
                _logger.LogError(ex, "Failed to send email to {To}", toEmail);
                return false;
            }
        }
    }
}

/*Reference list
Microsoft (n.d.) SmtpClient Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient (Accessed: 26 September 2026).
Microsoft(n.d.) MailMessage Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.mailmessage (Accessed: 26 September 2026).
Microsoft(n.d.) NetworkCredential Class. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.networkcredential (Accessed: 26 September 2026).
Microsoft(n.d.) Configuration in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/ (Accessed: 26 September 2026).
Microsoft(n.d.) Logging in .NET and ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/ (Accessed: 26 September 2026).
Microsoft(n.d.) Dependency injection in ASP.NET Core. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection (Accessed: 26 September 2026).
Klensin, J. (2008) RFC 5321: Simple Mail Transfer Protocol. Internet Engineering Task Force. Available at: https://www.rfc-editor.org/rfc/rfc5321 (Accessed: 26 September 2026). */
//------------------------------ END OF FILE ------------------------------//