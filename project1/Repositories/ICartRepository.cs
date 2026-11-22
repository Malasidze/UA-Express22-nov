using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines persisted customer cart operations.</summary>
    public interface ICartRepository
    {
        Cart Load(string userId, IMenuRepository menuRepository);
        void Save(string userId, Cart cart);
        void Clear(string userId);
    }
}
