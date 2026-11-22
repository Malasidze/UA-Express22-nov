using FoodExpress.Exceptions;

namespace FoodExpress.Models
{
    /// <summary>A durable snapshot of one item in an order.</summary>
    public sealed class OrderItem
    {
        public OrderItem(string itemId, string name, int quantity, decimal unitPrice)
        {
            if (quantity <= 0 || unitPrice < 0)
            {
                throw new ValidationException("Order item quantity and price are invalid.");
            }

            ItemId = itemId;
            Name = name;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public string ItemId { get; }
        public string Name { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
