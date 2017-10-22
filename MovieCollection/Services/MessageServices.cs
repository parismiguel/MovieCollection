using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MovieCollection.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Movie Collection", "paris.pantigoso@vcaperu.com"));
            emailMessage.To.Add(new MailboxAddress(email, email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("Html") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    UserName = "paris.pantigoso@vcaperu.com", // replace with valid value
                    Password = "ParisVCA2016" // replace with valid value
                };

                client.LocalDomain = "gmail.com";
                // check your smtp server setting and amend accordingly:
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.Auto).ConfigureAwait(false);
                await client.AuthenticateAsync(credentials);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
