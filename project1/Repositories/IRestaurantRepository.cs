using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines restaurant lookup operations.</summary>
    public interface IRestaurantRepository
    {
        IReadOnlyList<Restaurant> GetAll();
        Restaurant? GetById(string id);
    }
}
