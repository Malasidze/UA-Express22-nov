namespace FoodExpress.Models
{
    /// <summary>Represents a menu and order administrator.</summary>
    public sealed class Admin : User
    {
        public Admin(string id, string username, string passwordHash, string email)
            : base(id, username, passwordHash, email, Role.Admin)
        {
        }
    }
}
