using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines menu item lookup and persistence operations.</summary>
    public interface IMenuRepository
    {
        IReadOnlyList<MenuItem> GetAll();
        MenuItem? GetById(string id);
        void Save(MenuItem item);
        void Delete(string id);
    }
}
