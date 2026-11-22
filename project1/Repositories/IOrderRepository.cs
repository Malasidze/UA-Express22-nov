using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines order lookup and persistence operations.</summary>
    public interface IOrderRepository
    {
        IReadOnlyList<Order> GetAll();
        Order? GetById(string id);
        IReadOnlyList<Order> GetForUser(string userId);
        void Save(Order order);
    }
}
