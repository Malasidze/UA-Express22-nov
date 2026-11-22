using System.Net.Mail;
using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Registers users and authenticates credentials.</summary>
    public sealed class AuthService
    {
        private readonly IUserRepository _users;
        private readonly LoggerService _logger;

        public AuthService(IUserRepository users, LoggerService logger)
        {
            _users = users;
            _logger = logger;
        }

        public Customer Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("Username, email, and password are required.");
            }

            ValidateEmail(email);

            if (_users.FindByUsername(username) is not null || _users.FindByEmail(email) is not null)
            {
                throw new ValidationException("That username or email address is already registered.");
            }

            string id = NextCustomerId();
            Customer customer = new(id, username.Trim(), BCrypt.Net.BCrypt.HashPassword(password), email.Trim());
            _users.Add(customer);
            return customer;
        }

        public User Login(string email, string password)
        {
            ValidateEmail(email);
            User? user = _users.FindByEmail(email.Trim());
            bool passwordMatches = false;
            if (user is not null && !string.IsNullOrEmpty(password))
            {
                try
                {
                    passwordMatches = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
                catch (Exception ex)
                {
                    _logger.Log("ERROR", "PASSWORD_HASH_ERROR", ex.ToString());
                }
            }

            if (user is null || !passwordMatches)
            {
                _logger.Log("WARN", "LOGIN_FAILED", "Invalid email or password.");
                throw new InvalidCredentialsException();
            }

            _logger.Log("INFO", "LOGIN_SUCCESS", $"userId={user.Id}; role={user.Role}");
            return user;
        }

        private string NextCustomerId()
        {
            int next = _users.GetAll().Select(user => int.TryParse(user.Id, out int id) ? id : 1000).DefaultIfEmpty(1000).Max() + 1;
            return next.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                throw new InvalidEmailException();
            }

            try
            {
                _ = new MailAddress(email.Trim());
            }
            catch (FormatException)
            {
                throw new InvalidEmailException();
            }
        }
    }
}
