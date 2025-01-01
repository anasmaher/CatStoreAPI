using Core.Interfaces;
using Core.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {

        public void SendEmailAsync(EmailMetadata emailMetadata)
        {
            var fromMail = Environment.GetEnvironmentVariable("emailFrom");
            var password = Environment.GetEnvironmentVariable("emailPassword");
            var host = Environment.GetEnvironmentVariable("emailHost");
            
            var toMail = emailMetadata.ToAddress;
            var subject = emailMetadata.Subject;
            var body = emailMetadata.Body;

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromMail));
            email.To.Add(MailboxAddress.Parse(toMail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            smtp.Connect(host, 587, MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(fromMail, password);
            
            smtp.Send(email);

            smtp.Disconnect(true);
        }
    }
}