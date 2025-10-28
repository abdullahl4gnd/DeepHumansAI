using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DeepHumans.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // ✅ Replace these values with your own
            var fromAddress = new MailAddress("abodyfiras110@gmail.com", "AI DHumans Support");
            var toAddress = new MailAddress(email);
            const string fromPassword = "bfek yqlg wzrk tcwr"; // do NOT use your normal Gmail password

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            })
            {
                await smtp.SendMailAsync(message);
            }
        }
    }
}
