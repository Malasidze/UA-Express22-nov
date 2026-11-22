namespace FoodExpress.Models
{
    /// <summary>Represents an authenticated FoodExpress user.</summary>
    public abstract class User
    {
        protected User(string id, string username, string passwordHash, string email, Role role)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            Role = role;
        }

        public string Id { get; }
        public string Username { get; }
        public string PasswordHash { get; set; }
        public string Email { get; }
        public Role Role { get; }
    }
}
