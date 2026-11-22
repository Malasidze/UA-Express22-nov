using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Provides administrator-only menu maintenance operations.</summary>
    public sealed class MenuService
    {
        private readonly IMenuRepository _menu;

        public MenuService(IMenuRepository menu) => _menu = menu;

        public void Add(Admin admin, MenuItem item)
        {
            ArgumentNullException.ThrowIfNull(admin);
            if (item.Price <= 0) throw new ValidationException("Price must be greater than zero.");
            if (_menu.GetById(item.Id) is not null) throw new ValidationException("That menu item ID already exists.");
            _menu.Save(item);
        }

        public void Update(Admin admin, string itemId, string name, decimal price, bool isAvailable)
        {
            ArgumentNullException.ThrowIfNull(admin);
            if (price <= 0) throw new ValidationException("Price must be greater than zero.");
            MenuItem item = _menu.GetById(itemId) ?? throw new ValidationException("Menu item was not found.");
            item.Name = string.IsNullOrWhiteSpace(name) ? throw new ValidationException("Name is required.") : name.Trim();
            item.Price = price;
            item.IsAvailable = isAvailable;
            _menu.Save(item);
        }

        public void Disable(Admin admin, string itemId)
        {
            ArgumentNullException.ThrowIfNull(admin);
            MenuItem item = _menu.GetById(itemId) ?? throw new ValidationException("Menu item was not found.");
            item.IsAvailable = false;
            _menu.Save(item);
        }

        public void Delete(Admin admin, string itemId)
        {
            ArgumentNullException.ThrowIfNull(admin);
            if (_menu.GetById(itemId) is null) throw new ValidationException("Menu item was not found.");
            _menu.Delete(itemId);
        }
    }
}
