namespace FoodExpress.Models
{
    /// <summary>A restaurant that offers menu items.</summary>
    public sealed class Restaurant
    {
        public Restaurant(string id, string name, bool isOpen = true)
        {
            Id = id;
            Name = name;
            IsOpen = isOpen;
        }

        public string Id { get; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
    }
}
