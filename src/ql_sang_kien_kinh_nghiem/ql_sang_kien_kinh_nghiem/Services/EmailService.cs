using System.Net;
using System.Net.Mail;

namespace ql_sang_kien_kinh_nghiem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            try
            {
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"]!);
                var email = _configuration["EmailSettings:Email"];
                var password = _configuration["EmailSettings:Password"];
                var displayName = _configuration["EmailSettings:DisplayName"];

                using var smtp = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(email, password),
                    EnableSsl = true
                };

                var message = new MailMessage
                {
                    From = new MailAddress(email!, displayName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await smtp.SendMailAsync(message);

                _logger.LogInformation("Gửi email thành công");
            } catch
            {
                _logger.LogError("Lỗi gửi email");
            }
        }
    }
}