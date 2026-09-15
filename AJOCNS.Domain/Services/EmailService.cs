using AJOCNS.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AJOCNS.Domain.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            string? senderEmail = emailSettings["SenderEmail"];
            string smtpServer = emailSettings["SmtpServer"] ?? string.Empty;
            string? senderName = emailSettings["SenderName"];
            string? appPassword = emailSettings["AppPassword"];

            if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                throw new InvalidOperationException("Email settings (SenderEmail, SmtpServer, AppPassword) are not configured.");
            }

            if (!int.TryParse(emailSettings["Port"], out int port))
            {
                port = 587;
            }

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName ?? string.Empty),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, appPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
