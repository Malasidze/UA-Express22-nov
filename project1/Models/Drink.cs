namespace FoodExpress.Models
{
    /// <summary>A drink menu item.</summary>
    public sealed class Drink : MenuItem
    {
        public Drink(string id, string restaurantId, string name, string category, decimal price, string volume, bool isAvailable = true)
            : base(id, restaurantId, name, category, price, isAvailable)
        {
            Volume = volume;
        }

        public string Volume { get; set; }
        public override string GetDetails() => Volume;
    }
}
