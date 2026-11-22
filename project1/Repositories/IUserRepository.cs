using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines user lookup and persistence operations.</summary>
    public interface IUserRepository
    {
        IReadOnlyList<User> GetAll();
        User? GetById(string id);
        User? FindByUsername(string username);
        User? FindByEmail(string email);
        void Add(User user);
        void Update(User user);
    }
}
