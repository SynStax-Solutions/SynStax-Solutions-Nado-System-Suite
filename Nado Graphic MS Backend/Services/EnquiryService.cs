using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nado_Graphics_MS_Backend.Data;
using Nado_Graphics_MS_Backend.Models;

namespace Nado_Graphics_MS_Backend.Services
{
    public class EnquiryService
    {
        private readonly BMSDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        //Tracks recent submission timestamps per IP, in memory. Resets on app
        //restart and doesn't share state across multiple server instances       
        private static readonly ConcurrentDictionary<string, List<DateTime>> _submissionsByIp = new();
        private const int MaxSubmissionsPerWindow = 3;
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(10);

        public EnquiryService(BMSDbContext context, EmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }
        //----------------------------------------------------------------------------------
        //Returns false if the submission was rejected (validation, honeypot, or rate limit).
        //A honeypot hit returns true on purpose, so a bot can't tell its submission was dropped.
        public async Task<bool> SubmitEnquiryAsync(EnquiryRequest request, string ipAddress)
        {
            //Honeypot check - a filled in Website field means this it isn't a real person.
            //Silently succeeds without saving or emailing anything
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return false;
            }

            if (!IsValidEmail(request.Email))
            {
                return false;
            }

            if (!CheckRateLimit(ipAddress))
            {
                return false;
            }

            //Strips line breaks so a crafted multi-line Name can't inject extra
            //headers into the outgoing email's subject line.
            var safeName = request.Name.Replace("\r", " ").Replace("\n", " ").Trim();

            var enquiry = new Enquiry
            {
                Name = safeName,
                Email = request.Email.Trim(),
                Phone = request.Phone?.Trim(),
                Service = request.Service?.Trim(),
                Message = request.Message.Trim(),
                SubmittedAt = DateTime.Now,
                IsRead = false
            };

            _context.Enquiries.Add(enquiry);
            await _context.SaveChangesAsync();

            var paEmail = _configuration["PaInbox:Email"];

            if (!string.IsNullOrWhiteSpace(paEmail))
            {
                var subject = $"New website enquiry from {safeName}";
                var htmlBody = $"""
                <p><strong>Name:</strong> {System.Net.WebUtility.HtmlEncode(safeName)}</p>
                <p><strong>Email:</strong> {System.Net.WebUtility.HtmlEncode(enquiry.Email)}</p>
                <p><strong>Phone:</strong> {System.Net.WebUtility.HtmlEncode(enquiry.Phone ?? "-")}</p>
                <p><strong>Service:</strong> {System.Net.WebUtility.HtmlEncode(enquiry.Service ?? "-")}</p>
                <p><strong>Message:</strong></p>
                <p>{System.Net.WebUtility.HtmlEncode(enquiry.Message).Replace("\n", "<br/>")}</p>
                """;

                //Email failure doesn't roll back the saved enquiry - the PA can
                //still see it inside the BMS even if the notification email fails.
                try
                {
                    await _emailService.SendAsync(paEmail, subject, htmlBody);
                }
                catch
                {
                    // Intentionally swallowed, the enquiry is already saved either way.
                }
            }

            return true;
        }
        //----------------------------------------------------------------------------------
        public async Task<List<Enquiry>> GetEnquiriesAsync()
        {
            return await _context.Enquiries
                .OrderByDescending(e => e.SubmittedAt)
                .ToListAsync();
        }
        //----------------------------------------------------------------------------------
        public async Task<Enquiry?> GetEnquiryAsync(int id)
        {
            return await _context.Enquiries.FindAsync(id);
        }
        //----------------------------------------------------------------------------------
        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }
        //----------------------------------------------------------------------------------
        private static bool CheckRateLimit(string ipAddress)
        {
            var now = DateTime.UtcNow;
            var timestamps = _submissionsByIp.GetOrAdd(ipAddress, _ => new List<DateTime>());

            lock (timestamps)
            {
                timestamps.RemoveAll(t => now - t > RateLimitWindow);

                if (timestamps.Count >= MaxSubmissionsPerWindow)
                {
                    return false;
                }

                timestamps.Add(now);
                return true;
            }
        }
    }
}
//-------------------------------------------------END OF FILE------------------------------------------------------------