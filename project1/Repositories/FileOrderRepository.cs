using System.Globalization;
using FoodExpress.Data;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed order repository with an in-memory cache.</summary>
    public sealed class FileOrderRepository : IOrderRepository
    {
        private readonly FileManager _files;
        private readonly Action<string>? _log;
        private readonly List<Order> _orders = [];

        public FileOrderRepository(FileManager files, Action<string>? log = null)
        {
            _files = files;
            _log = log;
            Load();
        }

        public IReadOnlyList<Order> GetAll() => _orders.ToList();
        public Order? GetById(string id) => _orders.FirstOrDefault(order => order.Id == id);
        public IReadOnlyList<Order> GetForUser(string userId) => _orders.Where(order => order.UserId == userId).ToList();

        public void Save(Order order)
        {
            int index = _orders.FindIndex(existing => existing.Id == order.Id);
            if (index < 0) _orders.Add(order);
            else _orders[index] = order;
            Persist();
        }

        private void Load()
        {
            Dictionary<string, List<OrderItem>> itemsByOrder = new(StringComparer.Ordinal);
            foreach (string line in _files.ReadLines(FileManager.OrderItemsFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 6);
                    if (!int.TryParse(fields[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity)) throw new FormatException("Invalid order item quantity.");
                    OrderItem item = new(fields[1], fields[2], quantity, FileUserRepository.ParseMoney(fields[4]));
                    if (!itemsByOrder.TryGetValue(fields[0], out List<OrderItem>? orderItems)) itemsByOrder[fields[0]] = orderItems = [];
                    orderItems.Add(item);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed OrderItems.txt row '{line}': {ex.Message}");
                }
            }

            foreach (string line in _files.ReadLines(FileManager.OrdersFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 9);
                    DateTime createdAt = DateTime.ParseExact(fields[2], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    if (!Enum.TryParse(fields[8], true, out OrderStatus status)) throw new FormatException("Invalid order status.");
                    itemsByOrder.TryGetValue(fields[0], out List<OrderItem>? orderItems);
                    _orders.Add(new Order(fields[0], fields[1], createdAt, fields[3], FileUserRepository.ParseMoney(fields[4]), FileUserRepository.ParseMoney(fields[5]), FileUserRepository.ParseMoney(fields[6]), orderItems ?? [], status));
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed Orders.txt row '{line}': {ex.Message}");
                }
            }
        }

        private void Persist()
        {
            _files.WriteLines(FileManager.OrdersFile,
                new[] { "// OrderID | UserID | CreatedAt | Address | Subtotal | DeliveryFee | Discount | Total | Status" }
                .Concat(_orders.Select(order => $"{order.Id} | {order.UserId} | {order.CreatedAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} | {order.Address} | {FileUserRepository.Money(order.Subtotal)} | {FileUserRepository.Money(order.DeliveryFee)} | {FileUserRepository.Money(order.Discount)} | {FileUserRepository.Money(order.Total)} | {order.Status}")));
            _files.WriteLines(FileManager.OrderItemsFile,
                new[] { "// OrderID | ItemID | Name | Quantity | UnitPrice | LineTotal" }
                .Concat(_orders.SelectMany(order => order.Items.Select(item => $"{order.Id} | {item.ItemId} | {item.Name} | {item.Quantity} | {FileUserRepository.Money(item.UnitPrice)} | {FileUserRepository.Money(item.LineTotal)}"))));
        }
    }
}
