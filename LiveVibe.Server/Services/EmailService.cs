using System.Net.Mail;
using System.Net;

namespace LiveVibe.Server.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpClient = new SmtpClient(_config["Email:SmtpHost"])
            {
                Port = int.Parse(_config["Email:SmtpPort"]),
                Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true,
            };

            var message = new MailMessage
            {
                From = new MailAddress(_config["Email:Sender"]),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message);
        }
    }
}
