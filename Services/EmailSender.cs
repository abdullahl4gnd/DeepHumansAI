using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
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
            var senderName = _configuration["EmailSettings:SenderName"];
            var sendGridApiKey = _configuration["EmailSettings:SendGridApiKey"];

            Console.WriteLine("===============================================");
            Console.WriteLine($"📧 EMAIL SENDING ATTEMPT");
            Console.WriteLine($"TO: {email}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"SENDER: {senderEmail}");
            Console.WriteLine($"SendGrid API Key: {(!string.IsNullOrWhiteSpace(sendGridApiKey) ? "✓ Configured" : "✗ Not configured")}");
            Console.WriteLine("===============================================");

            // Prefer SendGrid if API key is configured
            if (!string.IsNullOrWhiteSpace(sendGridApiKey))
            {
                try
                {
                    var sgClient = new SendGridClient(sendGridApiKey);
                    var from = new EmailAddress(senderEmail, string.IsNullOrWhiteSpace(senderName) ? "DeepHumans Support" : senderName);
                    var to = new EmailAddress(email);
                    // Generate a basic plain text fallback
                    var plainText = System.Text.RegularExpressions.Regex.Replace(htmlMessage ?? string.Empty, "<[^>]*>", string.Empty);
                    var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlMessage);
                    var response = await sgClient.SendEmailAsync(msg);
                    
                    Console.WriteLine($"✅ SendGrid Response: {response.StatusCode}");
                    if (response.StatusCode != System.Net.HttpStatusCode.OK && 
                        response.StatusCode != System.Net.HttpStatusCode.Accepted)
                    {
                        var body = await response.Body.ReadAsStringAsync();
                        Console.WriteLine($"⚠️ SendGrid Error: {body}");
                    }
                    else
                    {
                        Console.WriteLine($"✅ Email sent successfully via SendGrid!");
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ SendGrid Error: {ex.Message}");
                    throw;
                }
            }

            // Fallback to SMTP
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var password = _configuration["EmailSettings:Password"];

            if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("⚠️ No email provider configured. Email not sent.");
                Console.WriteLine($"MESSAGE PREVIEW: {htmlMessage}");
                Console.WriteLine("===============================================");
                return;
            }

            try
            {
                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                Console.WriteLine("✅ Email sent successfully via SMTP!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SMTP Error: {ex.Message}");
                throw;
            }
        }
    }
}
