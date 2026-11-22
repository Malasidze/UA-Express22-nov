using System.Globalization;
using FoodExpress.Data;
using FoodExpress.Exceptions;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed user repository with an in-memory cache.</summary>
    public sealed class FileUserRepository : IUserRepository
    {
        private readonly FileManager _files;
        private readonly Action<string>? _log;
        private readonly List<User> _users = [];
        private readonly HashSet<string> _unverifiedIds = new(StringComparer.Ordinal);

        public FileUserRepository(FileManager files, Action<string>? log = null)
        {
            _files = files;
            _log = log;
            Load();
        }

        public IReadOnlyList<User> GetAll() => _users.ToList();
        public User? GetById(string id) => _users.FirstOrDefault(user => user.Id == id);
        public User? FindByUsername(string username) => _users.FirstOrDefault(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
        public User? FindByEmail(string email) => _users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));

        public void Add(User user)
        {
            if (FindByUsername(user.Username) is not null || FindByEmail(user.Email) is not null)
            {
                throw new ValidationException("That username or email address is already registered.");
            }

            _users.Add(user);
            Persist();
            UpdateVerification(user);
        }

        public void Update(User user)
        {
            int index = _users.FindIndex(existing => existing.Id == user.Id);
            if (index < 0)
            {
                throw new ValidationException("User was not found.");
            }

            if (_users.Any(existing => existing.Id != user.Id && (string.Equals(existing.Username, user.Username, StringComparison.OrdinalIgnoreCase) || string.Equals(existing.Email, user.Email, StringComparison.OrdinalIgnoreCase))))
            {
                throw new ValidationException("That username or email address is already registered.");
            }

            _users[index] = user;
            Persist();
            UpdateVerification(user);
        }

        private void Load()
        {
            bool changed = false;
            foreach (string line in _files.ReadLines(FileManager.EmailVerificationFile))
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    _unverifiedIds.Add(line.Trim());
                }
            }

            foreach (string line in _files.ReadLines(FileManager.UsersFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                try
                {
                    string[] fields = Split(line, 5);
                    string id = fields[0];
                    string username = fields[1];
                    string password = fields[2];
                    string email = fields[3];
                    if (!password.StartsWith("$2", StringComparison.Ordinal))
                    {
                        password = BCrypt.Net.BCrypt.HashPassword(password);
                        changed = true;
                    }

                    User user = fields[4].Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
                        ? new Admin(id, username, password, email)
                        : new Customer(id, username, password, email, isEmailVerified: !_unverifiedIds.Contains(id));
                    _users.Add(user);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed Users.txt row '{line}': {ex.Message}");
                }
            }

            if (changed)
            {
                Persist();
            }
        }

        private void Persist() => _files.WriteLines(FileManager.UsersFile,
            new[] { "// ID | Username | Password | Email | Role" }.Concat(_users.Select(user =>
                $"{user.Id} | {user.Username} | {user.PasswordHash} | {user.Email} | {user.Role.ToString().ToLowerInvariant()}")));

        private void UpdateVerification(User user)
        {
            if (user is not Customer customer) return;
            if (customer.IsEmailVerified) _unverifiedIds.Remove(customer.Id);
            else _unverifiedIds.Add(customer.Id);
            _files.WriteLines(FileManager.EmailVerificationFile, new[] { "// UserID" }.Concat(_unverifiedIds));
        }

        internal static string[] Split(string line, int expectedCount)
        {
            string[] fields = line.Split('|').Select(field => field.Trim()).ToArray();
            if (fields.Length != expectedCount)
            {
                throw new FormatException($"Expected {expectedCount} pipe-delimited fields.");
            }

            return fields;
        }

        internal static decimal ParseMoney(string value) => decimal.Parse(value.Replace("₾", string.Empty, StringComparison.Ordinal).Trim(), CultureInfo.InvariantCulture);
        internal static string Money(decimal value) => $"{value.ToString("0.00", CultureInfo.InvariantCulture)} ₾";
    }
}
