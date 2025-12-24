using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DeepHumans.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"] ?? "DeepHumans Support";
            var sendGridApiKey = _configuration["EmailSettings:SendGridApiKey"];

            Console.WriteLine("===============================================");
            Console.WriteLine("📧 EMAIL SENDING ATTEMPT");
            Console.WriteLine($"TO: {email}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"SENDER: {senderEmail}");
            Console.WriteLine($"SendGrid: {(string.IsNullOrWhiteSpace(sendGridApiKey) ? "❌ Disabled" : "✅ Enabled")}");
            Console.WriteLine("===============================================");

            // 1️ SENDGRID (PREFERRED)
            if (!string.IsNullOrWhiteSpace(sendGridApiKey))
            {
                try
                {
                    var client = new SendGridClient(sendGridApiKey);
                    var from = new EmailAddress(senderEmail, senderName);
                    var to = new EmailAddress(email);

                    var plainText = Regex.Replace(htmlMessage ?? string.Empty, "<[^>]+>", "");

                    var msg = MailHelper.CreateSingleEmail(
                        from,
                        to,
                        subject,
                        plainText,
                        htmlMessage
                    );

                    var response = await client.SendEmailAsync(msg);

                    Console.WriteLine($"📨 SendGrid Status: {response.StatusCode}");

                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                        response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine("✅ Email sent successfully via SendGrid.");
                        return;
                    }

                    var errorBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"⚠️ SendGrid Error Body: {errorBody}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ SendGrid Exception: {ex.Message}");
                    // Continue to SMTP fallback
                }
            }

            // 2️⃣ SMTP FALLBACK (GMAIL)
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
            var smtpPassword = _configuration["EmailSettings:Password"];

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(smtpPassword))
            {
                Console.WriteLine("❌ SMTP is not configured. Email NOT sent.");
                Console.WriteLine("MESSAGE CONTENT:");
                Console.WriteLine(htmlMessage);
                return;
            }

            try
            {
                using var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, smtpPassword),
                    EnableSsl = true
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mail.To.Add(email);

                await smtpClient.SendMailAsync(mail);

                Console.WriteLine("✅ Email sent successfully via SMTP.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SMTP Exception: {ex.Message}");
                throw;
            }
        }
    }
}
