using MailKit.Net.Smtp; // connects to Gmail and sends the email
using MailKit.Security; // provides secure connection options such as StartTls
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options; // reads your Gmail configuration from appsettings.json
using MimeKit; // constructs the email itself - sender, recipient, subject, body, etc.

namespace RegistrationSystem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly GmailOptions _options; // storing the gmail configuration
                                                // _options holds the settings from UserSecrets instead of being written directly in the code
        
        // constructor dependency injection
        public EmailSender(
            IOptions<GmailOptions> options)
        {
            _options = options.Value;
        }

        // email sending method
        public async Task SendEmailAsync( 
            string email, // recipient email address
            string subject, // email subject
            string htmlMessage) // email body in HTML format
        {
            if (string.IsNullOrWhiteSpace( // checks if the Gmail address exists in the configuration
                    _options.Username))
            {
                throw new InvalidOperationException(
                    "The Gmail username is not configured.");
            }

            if (string.IsNullOrWhiteSpace( // same thing
                    _options.AppPassword))
            {
                throw new InvalidOperationException(
                    "The Gmail App Password is not configured.");
            }

            var message = new MimeMessage(); // MimeMessage represents the complete email being constructed

            // setting the sender
            message.From.Add( 
                new MailboxAddress(
                    _options.FromName, // the name shown to the recipient
                    _options.Username)); // the Gmail address sending the message

            // setting the recipient
            message.To.Add(
                MailboxAddress.Parse(email));

            // setting the subject
            message.Subject = subject;

            // creating the email body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage, // allows clickable links and formatting
                TextBody = htmlMessage // plain text version of the email body
            };

            // this converts the BodyBuilder into the final format required by MimeMessage
            message.Body =
                bodyBuilder.ToMessageBody();

            // creating the SMTP client
            using var smtpClient = new SmtpClient(); // SMTP client communicates with Gmail

            // Gmail SMTP uses STARTTLS on port 587 (connecting securely to Gmail)
            await smtpClient.ConnectAsync(
                _options.Host, // found in UserSecrets
                _options.Port, // found in UserSecrets
                SecureSocketOptions.StartTls); // means the client establishes a connection and then upgrades it to an encryptes TLS connection befrore sending sensitive info

            // Google requires the App Password here,
            // not the normal Gmail password.
            await smtpClient.AuthenticateAsync(
                _options.Username,
                _options.AppPassword.Replace(
                    " ", 
                    string.Empty)); // this code removes the empty spaces from the password

            await smtpClient.SendAsync(message); // sending the email

            await smtpClient.DisconnectAsync( // disconnecting from the SMTP server
                true);
        }
    }
}