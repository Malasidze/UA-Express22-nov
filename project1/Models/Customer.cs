namespace FoodExpress.Models
{
    /// <summary>Represents a customer with a cart and order history.</summary>
    public sealed class Customer : User
    {
        public Customer(string id, string username, string passwordHash, string email, bool isEmailVerified = false)
            : base(id, username, passwordHash, email, Role.Customer)
        {
            IsEmailVerified = isEmailVerified;
        }

        public Cart Cart { get; } = new();
        public List<Order> Orders { get; } = [];
        public bool IsEmailVerified { get; set; }
    }
}
