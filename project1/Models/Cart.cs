using FoodExpress.Exceptions;

namespace FoodExpress.Models
{
    /// <summary>A customer's mutable shopping cart.</summary>
    public sealed class Cart
    {
        private readonly List<CartItem> _items = [];
        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public decimal TotalPrice => _items.Sum(item => item.Item.GetFinalPrice() * item.Quantity);

        public void AddItem(MenuItem item, int quantity)
        {
            ArgumentNullException.ThrowIfNull(item);
            ValidateQuantity(quantity);
            if (!item.IsAvailable)
            {
                throw new ItemUnavailableException($"'{item.Name}' is not currently available.");
            }

            CartItem? existing = _items.FirstOrDefault(entry => entry.Item.Id == item.Id);
            if (existing is null)
            {
                _items.Add(new CartItem(item, quantity));
            }
            else
            {
                existing.SetQuantity(checked(existing.Quantity + quantity));
            }
        }

        public void RemoveItem(string itemId)
        {
            CartItem? item = _items.FirstOrDefault(entry => entry.Item.Id == itemId);
            if (item is null)
            {
                throw new ValidationException("That item is not in the cart.");
            }

            _items.Remove(item);
        }

        public void ChangeQuantity(string itemId, int newQuantity)
        {
            ValidateQuantity(newQuantity);
            CartItem? item = _items.FirstOrDefault(entry => entry.Item.Id == itemId);
            if (item is null)
            {
                throw new ValidationException("That item is not in the cart.");
            }

            item.SetQuantity(newQuantity);
        }

        public void Clear() => _items.Clear();

        private static void ValidateQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                throw new ValidationException("Quantity must be a positive whole number.");
            }
        }
    }
}
