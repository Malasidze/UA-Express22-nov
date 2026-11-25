using FoodExpress.Exceptions;

namespace FoodExpress.Models
{
    /// <summary>Base type for restaurant menu items.</summary>
    public abstract class MenuItem
    {
        protected MenuItem(string id, string restaurantId, string name, string category, decimal price, bool isAvailable = true)
        {
            if (price <= 0)
            {
                throw new ValidationException("Price must be greater than zero.");
            }

            Id = id;
            RestaurantId = restaurantId;
            Name = name;
            Category = category;
            Price = price;
            IsAvailable = isAvailable;
        }

        public string Id { get; }
        public string RestaurantId { get; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public virtual string TypeName => GetType().Name;
        public abstract string GetDetails();
        public virtual decimal GetFinalPrice() => Price;

        public virtual void DisplayDetails()
        {
            System.Diagnostics.Trace.WriteLine($"{Id}: {Name} ({Category}) - {Price:0.00} ₾ - {GetDetails()}");
        }
    }
}
