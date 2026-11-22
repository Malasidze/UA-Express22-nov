namespace FoodExpress.Models
{
    /// <summary>A dessert menu item.</summary>
    public sealed class Dessert : MenuItem
    {
        public Dessert(string id, string restaurantId, string name, string category, decimal price, string portionSize, bool isAvailable = true)
            : base(id, restaurantId, name, category, price, isAvailable)
        {
            PortionSize = portionSize;
        }

        public string PortionSize { get; set; }
        public override string GetDetails() => PortionSize;
    }
}
