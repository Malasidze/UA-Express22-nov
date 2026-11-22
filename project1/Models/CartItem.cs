namespace FoodExpress.Models
{
    /// <summary>A cart line whose quantity can only be changed by its cart.</summary>
    public sealed class CartItem
    {
        internal CartItem(MenuItem item, int quantity)
        {
            Item = item;
            _quantity = quantity;
        }

        private int _quantity;
        public MenuItem Item { get; }
        public int Quantity => _quantity;
        internal void SetQuantity(int quantity) => _quantity = quantity;
    }
}
