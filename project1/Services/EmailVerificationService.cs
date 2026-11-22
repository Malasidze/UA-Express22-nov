using System.Security.Cryptography;
using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Issues expiring email verification codes and activates verified accounts.</summary>
    public sealed class EmailVerificationService
    {
        private sealed record Verification(string Code, DateTime ExpiresAt, DateTime LastSentAt);

        private readonly Dictionary<string, Verification> _verifications = new(StringComparer.Ordinal);
        private readonly EmailService _email;
        private readonly IUserRepository _users;
        private readonly LoggerService _logger;

        public EmailVerificationService(EmailService email, IUserRepository users, LoggerService logger)
        {
            _email = email;
            _users = users;
            _logger = logger;
        }

        public async Task SendCodeAsync(Customer customer, bool isResend = false)
        {
            DateTime now = DateTime.UtcNow;
            if (isResend && _verifications.TryGetValue(customer.Id, out Verification? current) && now - current.LastSentAt < TimeSpan.FromMinutes(1))
            {
                throw new ValidationException("You can request another code once per minute.");
            }

            string code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
            _verifications[customer.Id] = new Verification(code, now.AddMinutes(10), now);
            await _email.SendAsync(customer.Email, "FoodExpress email verification", $"<p>Your FoodExpress verification code is <strong>{code}</strong>. It expires in 10 minutes.</p>");
        }

        public bool Verify(Customer customer, string code)
        {
            if (!_verifications.TryGetValue(customer.Id, out Verification? verification) ||
                verification.ExpiresAt < DateTime.UtcNow ||
                !CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(verification.Code), System.Text.Encoding.UTF8.GetBytes(code.Trim())))
            {
                throw new ValidationException("The verification code is invalid or expired.");
            }

            customer.IsEmailVerified = true;
            _users.Update(customer);
            _verifications.Remove(customer.Id);
            _logger.Log("INFO", "EMAIL_VERIFIED", $"userId={customer.Id}");
            return true;
        }
    }
}
