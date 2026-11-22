using System.Globalization;
using System.Net;
using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Places orders, enforces ownership, and advances the order lifecycle.</summary>
    public sealed class OrderService
    {
        private const decimal StandardDeliveryFee = 4.00m;
        private readonly IOrderRepository _orders;
        private readonly ICartRepository _carts;
        private readonly PromoCodeService _promos;
        private readonly EmailService _email;
        private readonly LoggerService _logger;

        public OrderService(IOrderRepository orders, ICartRepository carts, PromoCodeService promos, EmailService email, LoggerService logger)
        {
            _orders = orders;
            _carts = carts;
            _promos = promos;
            _email = email;
            _logger = logger;
        }

        public bool LastEmailWarning { get; private set; }

        public async Task<Order> PlaceOrder(Customer customer, string address, string? promoCode = null)
        {
            if (customer.Cart.Items.Count == 0) throw new EmptyCartException();
            if (string.IsNullOrWhiteSpace(address)) throw new ValidationException("A delivery address is required.");
            if (!customer.IsEmailVerified) throw new AuthorizationException("Verify your email address before placing an order.");

            decimal subtotal = customer.Cart.Items.Sum(item => item.Item.GetFinalPrice() * item.Quantity);
            decimal discount = _promos.CalculateDiscount(promoCode, subtotal);
            string orderId = NextOrderId();
            List<OrderItem> items = customer.Cart.Items
                .Select(item => new OrderItem(item.Item.Id, item.Item.Name, item.Quantity, item.Item.GetFinalPrice()))
                .ToList();
            Order order = new(orderId, customer.Id, DateTime.Now, address.Trim(), subtotal, StandardDeliveryFee, discount, items);

            _orders.Save(order);
            if (!string.IsNullOrWhiteSpace(promoCode)) _promos.MarkUsed(promoCode, order.Id);
            _carts.Clear(customer.Id);
            customer.Cart.Clear();
            customer.Orders.Add(order);
            _logger.Log("INFO", "ORDER_CREATED", $"orderId={order.Id}; userId={customer.Id}; total={order.Total.ToString("0.00", CultureInfo.InvariantCulture)}");

            try
            {
                await _email.SendAsync(customer.Email, $"FoodExpress order {order.Id}", BuildReceipt(order));
                LastEmailWarning = false;
            }
            catch (EmailDeliveryException)
            {
                LastEmailWarning = true;
            }

            return order;
        }

        public IReadOnlyList<Order> GetMyOrders(Customer customer) => _orders.GetForUser(customer.Id);

        public Order GetMyOrder(Customer customer, string orderId)
        {
            Order order = _orders.GetById(orderId) ?? throw new ValidationException("Order was not found.");
            if (order.UserId != customer.Id) throw new AuthorizationException("You can only access your own orders.");
            return order;
        }

        public void CancelOrder(Customer customer, string orderId)
        {
            Order order = GetMyOrder(customer, orderId);
            order.Cancel();
            _orders.Save(order);
            _logger.Log("INFO", "ORDER_CANCELLED", $"orderId={order.Id}; userId={customer.Id}");
        }

        public IReadOnlyList<Order> GetAllOrders(Admin admin)
        {
            ArgumentNullException.ThrowIfNull(admin);
            return _orders.GetAll();
        }

        public void ConfirmOrder(Admin admin, string orderId) => ChangeStatus(admin, orderId, OrderStatus.Created, "ORDER_CONFIRMED", order => order.Confirm());
        public void StartPreparing(Admin admin, string orderId) => ChangeStatus(admin, orderId, OrderStatus.Confirmed, "ORDER_PREPARING", order => order.StartPreparing());
        public void SendForDelivery(Admin admin, string orderId) => ChangeStatus(admin, orderId, OrderStatus.Preparing, "ORDER_ON_THE_WAY", order => order.SendForDelivery());
        public void CompleteDelivery(Admin admin, string orderId) => ChangeStatus(admin, orderId, OrderStatus.OnTheWay, "ORDER_DELIVERED", order => order.CompleteDelivery());

        private void ChangeStatus(Admin admin, string orderId, OrderStatus expected, string eventName, Action<Order> transition)
        {
            ArgumentNullException.ThrowIfNull(admin);
            Order order = _orders.GetById(orderId) ?? throw new ValidationException("Order was not found.");
            if (order.Status != expected)
            {
                throw new InvalidOrderStatusTransitionException($"The next valid action for this order is determined by its current status ({order.Status}).");
            }

            transition(order);
            _orders.Save(order);
            _logger.Log("INFO", eventName, $"orderId={order.Id}; adminId={admin.Id}");
        }

        private string NextOrderId()
        {
            int next = _orders.GetAll()
                .Select(order => int.TryParse(order.Id.TrimStart('O', 'o'), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : 900)
                .DefaultIfEmpty(900)
                .Max() + 1;
            return "O" + next.ToString(CultureInfo.InvariantCulture);
        }

        private static string BuildReceipt(Order order)
        {
            string lines = string.Join("", order.Items.Select(item =>
                $"<tr><td>{WebUtility.HtmlEncode(item.Name)}</td><td>{item.Quantity}</td><td>{item.UnitPrice:0.00} ₾</td><td>{item.LineTotal:0.00} ₾</td></tr>"));
            return $"<h2>Order {WebUtility.HtmlEncode(order.Id)}</h2><p>Delivery address: {WebUtility.HtmlEncode(order.Address)}</p>" +
                   $"<table><thead><tr><th>Item</th><th>Qty</th><th>Unit price</th><th>Total</th></tr></thead><tbody>{lines}</tbody></table>" +
                   $"<p>Subtotal: {order.Subtotal:0.00} ₾</p><p>Delivery fee: {order.DeliveryFee:0.00} ₾</p>" +
                   $"<p>Discount: {order.Discount:0.00} ₾</p><strong>Total: {order.Total:0.00} ₾</strong>";
        }
    }
}
