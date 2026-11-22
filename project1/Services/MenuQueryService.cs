using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>A menu item sales summary.</summary>
    public sealed record ItemSales(string ItemId, string Name, int Quantity, int OrderLineCount, decimal Revenue);

    /// <summary>Provides LINQ-based menu filtering and sales statistics.</summary>
    public sealed class MenuQueryService
    {
        private readonly IMenuRepository _menu;
        private readonly IOrderRepository _orders;

        public MenuQueryService(IMenuRepository menu, IOrderRepository orders)
        {
            _menu = menu;
            _orders = orders;
        }

        public IReadOnlyList<MenuItem> Filter(decimal? minPrice = null, decimal? maxPrice = null, string? category = null, string? restaurantId = null)
        {
            IEnumerable<MenuItem> query = _menu.GetAll().Where(item => item.IsAvailable);
            if (minPrice.HasValue) query = query.Where(item => item.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(item => item.Price <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(restaurantId)) query = query.Where(item => item.RestaurantId.Equals(restaurantId, StringComparison.OrdinalIgnoreCase));
            return query.OrderBy(item => item.Name).ToList();
        }

        public MenuItem? GetBestSellingItem()
        {
            MenuItem? best = _orders.GetAll().Where(order => order.Status != OrderStatus.Cancelled)
                .SelectMany(order => order.Items)
                .GroupBy(item => item.ItemId)
                .Select(group => new { Id = group.Key, Qty = group.Sum(item => item.Quantity), Count = group.Count() })
                .OrderByDescending(item => item.Qty)
                .Select(item => _menu.GetById(item.Id))
                .FirstOrDefault(item => item is not null);
            return best;
        }

        public IReadOnlyList<ItemSales> GetSalesStatistics()
        {
            return _orders.GetAll().Where(order => order.Status != OrderStatus.Cancelled)
                .SelectMany(order => order.Items)
                .GroupBy(item => new { item.ItemId, item.Name })
                .Select(group => new ItemSales(group.Key.ItemId, group.Key.Name, group.Sum(item => item.Quantity), group.Count(), group.Sum(item => item.LineTotal)))
                .OrderByDescending(item => item.Revenue)
                .ToList();
        }
    }
}
