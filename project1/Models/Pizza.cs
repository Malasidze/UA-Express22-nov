namespace FoodExpress.Models
{
    /// <summary>A pizza menu item.</summary>
    public sealed class Pizza : MenuItem
    {
        public Pizza(string id, string restaurantId, string name, string category, decimal price, string size, string dough, bool isAvailable = true)
            : base(id, restaurantId, name, category, price, isAvailable)
        {
            Size = size;
            Dough = dough;
        }

        public string Size { get; set; }
        public string Dough { get; set; }
        public override string GetDetails() => $"{Size};{Dough}";
    }
}
