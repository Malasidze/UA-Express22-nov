using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Provides administrator-only restaurant maintenance operations.</summary>
    public sealed class RestaurantService
    {
        private readonly IRestaurantRepository _restaurants;

        public RestaurantService(IRestaurantRepository restaurants) => _restaurants = restaurants;

        public void Add(Admin admin, string id, string name, bool isOpen)
        {
            ArgumentNullException.ThrowIfNull(admin);
            id = id.Trim();
            name = name.Trim();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Restaurant ID and name are required.");
            if (_restaurants.GetById(id) is not null)
                throw new ValidationException("That restaurant ID already exists.");

            _restaurants.Save(new Restaurant(id, name, isOpen));
        }

        public void Update(Admin admin, string id, string name, bool isOpen)
        {
            ArgumentNullException.ThrowIfNull(admin);
            Restaurant restaurant = _restaurants.GetById(id) ?? throw new ValidationException("Restaurant was not found.");
            if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Restaurant name is required.");
            restaurant.Name = name.Trim();
            restaurant.IsOpen = isOpen;
            _restaurants.Save(restaurant);
        }
    }
}