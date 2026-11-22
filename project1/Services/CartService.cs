using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Loads and persists customer cart changes.</summary>
    public sealed class CartService
    {
        private readonly ICartRepository _carts;
        private readonly IMenuRepository _menu;

        public CartService(ICartRepository carts, IMenuRepository menu)
        {
            _carts = carts;
            _menu = menu;
        }

        public void Load(Customer customer)
        {
            customer.Cart.Clear();
            Cart saved = _carts.Load(customer.Id, _menu);
            foreach (CartItem item in saved.Items)
            {
                customer.Cart.AddItem(item.Item, item.Quantity);
            }
        }

        public void AddItem(Customer customer, MenuItem item, int quantity)
        {
            customer.Cart.AddItem(item, quantity);
            Save(customer);
        }

        public void RemoveItem(Customer customer, string itemId)
        {
            customer.Cart.RemoveItem(itemId);
            Save(customer);
        }

        public void ChangeQuantity(Customer customer, string itemId, int quantity)
        {
            customer.Cart.ChangeQuantity(itemId, quantity);
            Save(customer);
        }

        public void Clear(Customer customer)
        {
            customer.Cart.Clear();
            _carts.Clear(customer.Id);
        }

        public void Save(Customer customer) => _carts.Save(customer.Id, customer.Cart);

        public static void EnsureCustomer(User user)
        {
            if (user is not Customer)
            {
                throw new AuthorizationException("Only customers can use a shopping cart.");
            }
        }
    }
}
