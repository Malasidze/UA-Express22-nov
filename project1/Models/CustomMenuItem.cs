namespace FoodExpress.Models
{
    /// <summary>A restaurant-specific menu item type with free-form details.</summary>
    public sealed class CustomMenuItem : MenuItem
    {
        public CustomMenuItem(string id, string restaurantId, string typeName, string name, string category, decimal price, string details, bool isAvailable = true)
            : base(id, restaurantId, name, category, price, isAvailable)
        {
            TypeName = typeName;
            Details = details;
        }

        public override string TypeName { get; }
        public string Details { get; set; }
        public override string GetDetails() => Details;
    }
}