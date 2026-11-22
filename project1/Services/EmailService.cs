using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using FoodExpress.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FoodExpress.Services
{
    /// <summary>Sends HTML email through the configured SMTP server.</summary>
    public sealed class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly LoggerService _logger;

        public EmailService(EmailSettings settings, LoggerService logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            using SmtpClient client = new();
            bool connected = false;
            try
            {
                MimeMessage message = new();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                await client.ConnectAsync(_settings.Server, _settings.Port, SecureSocketOptions.StartTls);
                connected = true;
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
                await client.SendAsync(new SmtpSendRequest(message));
                await client.DisconnectAsync(true);
                connected = false;
                _logger.Log("INFO", "EMAIL_SENT", $"to={toEmail}; subject={subject}");
            }
            catch (Exception ex) when (ex is SmtpCommandException or SmtpProtocolException or System.Security.Authentication.AuthenticationException or MailKit.Security.AuthenticationException or SocketException)
            {
                _logger.Log("ERROR", "EMAIL_FAILED", $"to={toEmail}; {ex}");
                throw new EmailDeliveryException("The email could not be delivered.", ex);
            }
            catch (Exception ex)
            {
                _logger.Log("ERROR", "EMAIL_FAILED", $"to={toEmail}; {ex}");
                throw new EmailDeliveryException("The email could not be delivered.", ex);
            }
            finally
            {
                if (connected)
                {
                    try
                    {
                        await client.DisconnectAsync(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log("ERROR", "EMAIL_FAILED", $"SMTP disconnect failed: {ex}");
                    }
                }
            }
        }
    }
}
