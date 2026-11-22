using FoodExpress.Exceptions;

namespace FoodExpress.Models
{
    /// <summary>An order with validated status transitions.</summary>
    public sealed class Order
    {
        public Order(string id, string userId, DateTime createdAt, string address, decimal subtotal, decimal deliveryFee, decimal discount, IEnumerable<OrderItem> items, OrderStatus status = OrderStatus.Created)
        {
            if (subtotal < 0 || deliveryFee < 0 || discount < 0 || discount > subtotal)
            {
                throw new ValidationException("Order totals are invalid.");
            }

            Id = id;
            UserId = userId;
            CreatedAt = createdAt;
            Address = address;
            Subtotal = subtotal;
            DeliveryFee = deliveryFee;
            Discount = discount;
            Total = subtotal + deliveryFee - discount;
            Items = items.ToList();
            Status = status;
        }

        public string Id { get; }
        public string UserId { get; }
        public DateTime CreatedAt { get; }
        public string Address { get; }
        public decimal Subtotal { get; }
        public decimal DeliveryFee { get; }
        public decimal Discount { get; }
        public decimal Total { get; }
        public OrderStatus Status { get; private set; }
        public List<OrderItem> Items { get; }

        public void Confirm() => Transition(OrderStatus.Created, OrderStatus.Confirmed);
        public void StartPreparing() => Transition(OrderStatus.Confirmed, OrderStatus.Preparing);
        public void SendForDelivery() => Transition(OrderStatus.Preparing, OrderStatus.OnTheWay);
        public void CompleteDelivery() => Transition(OrderStatus.OnTheWay, OrderStatus.Delivered);

        public void Cancel()
        {
            if (Status is not (OrderStatus.Created or OrderStatus.Confirmed))
            {
                throw new InvalidOrderStatusTransitionException($"An order in status {Status} cannot be cancelled.");
            }

            Status = OrderStatus.Cancelled;
        }

        private void Transition(OrderStatus expected, OrderStatus next)
        {
            if (Status != expected)
            {
                throw new InvalidOrderStatusTransitionException($"Cannot change order status from {Status} to {next}.");
            }

            Status = next;
        }
    }
}
