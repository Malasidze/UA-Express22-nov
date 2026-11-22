using System.Globalization;
using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;
using FoodExpress.Services;
using static FoodExpress.Menu.MenuInput;

namespace FoodExpress.Menu
{
    /// <summary>Provides customer menu browsing, cart, ordering, and order-history workflows.</summary>
    public sealed class CustomerMenu
    {
        private readonly MenuQueryService _menuQuery;
        private readonly IMenuRepository _menu;
        private readonly IRestaurantRepository _restaurants;
        private readonly CartService _cartService;
        private readonly OrderService _orders;
        private readonly LoggerService _logger;

        public CustomerMenu(MenuQueryService menuQuery, IMenuRepository menu, IRestaurantRepository restaurants, CartService cartService, OrderService orders, LoggerService logger)
        {
            _menuQuery = menuQuery;
            _menu = menu;
            _restaurants = restaurants;
            _cartService = cartService;
            _orders = orders;
            _logger = logger;
        }

        public void LoadCart(Customer customer) => _cartService.Load(customer);

        public async Task RunAsync(Customer customer)
        {
            while (true)
            {
                int choice = Choose($"Customer: {customer.Username}",
                    (1, "Browse restaurants and menu"), (2, "Filter menu"), (3, "Add item to cart"),
                    (4, "View/modify cart"), (5, "Place order"), (6, "View my orders"),
                    (7, "Cancel order"), (0, "Logout"));
                switch (choice)
                {
                    case 1:
                        Browse();
                        break;
                    case 2:
                        Filter();
                        break;
                    case 3:
                        AddToCart(customer);
                        break;
                    case 4:
                        ModifyCart(customer);
                        break;
                    case 5:
                        await PlaceOrderAsync(customer);
                        break;
                    case 6:
                        ViewOrders(customer);
                        break;
                    case 7:
                        CancelOrder(customer);
                        break;
                    case 0:
                        return;
                }
            }
        }

        private void Browse()
        {
            try
            {
                Console.WriteLine("Restaurants:");
                foreach (Restaurant restaurant in _restaurants.GetAll())
                    Console.WriteLine($"{restaurant.Id}: {restaurant.Name} ({(restaurant.IsOpen ? "open" : "closed")})");
                PrintItems(_menuQuery.Filter());
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void Filter()
        {
            try
            {
                decimal? min = ReadOptionalDecimal("Minimum price (blank for any): ");
                decimal? max = ReadOptionalDecimal("Maximum price (blank for any): ");
                string category = ReadText("Category (blank for any): ");
                string restaurant = ReadText("Restaurant ID (blank for any): ");
                PrintItems(_menuQuery.Filter(min, max, category, restaurant));
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private void AddToCart(Customer customer)
        {
            try
            {
                PrintItems(_menuQuery.Filter());
                string itemId = ReadText("Item ID: ");
                MenuItem item = _menu.GetById(itemId) ?? throw new ValidationException("Menu item was not found.");
                int quantity = ReadPositiveInt("Quantity: ");
                _cartService.AddItem(customer, item, quantity);
                Console.WriteLine("Item added to your cart.");
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private void ModifyCart(Customer customer)
        {
            while (true)
            {
                ShowCart(customer);
                try
                {
                    int choice = Choose("Cart",
                        (1, "Change quantity"), (2, "Remove item"), (3, "Clear cart"), (0, "Back"));
                    switch (choice)
                    {
                        case 1:
                            _cartService.ChangeQuantity(customer, ReadText("Item ID: "), ReadPositiveInt("New quantity: "));
                            break;
                        case 2:
                            _cartService.RemoveItem(customer, ReadText("Item ID: "));
                            break;
                        case 3:
                            _cartService.Clear(customer);
                            Console.WriteLine("Cart cleared.");
                            break;
                        case 0:
                            return;
                    }
                }
                catch (Exception ex) when (ex is not EndOfInputException)
                {
                    ShowError(ex);
                }
            }
        }

        private async Task PlaceOrderAsync(Customer customer)
        {
            try
            {
                string address = ReadText("Delivery address: ");
                string promo = ReadText("Promo code (blank for none): ");
                Order order = await _orders.PlaceOrder(customer, address, promo);
                Console.WriteLine($"Order {order.Id} placed. Total: {order.Total:0.00} ₾. Status: {order.Status}.");
                if (_orders.LastEmailWarning) Console.WriteLine("Warning: the order was placed, but the confirmation email could not be sent.");
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private void ViewOrders(Customer customer)
        {
            try
            {
                IReadOnlyList<Order> orders = _orders.GetMyOrders(customer);
                if (orders.Count == 0) Console.WriteLine("You have no orders.");
                foreach (Order order in orders)
                    Console.WriteLine($"{order.Id} | {order.CreatedAt:g} | {order.Status} | {order.Total:0.00} ₾ | {order.Address}");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void CancelOrder(Customer customer)
        {
            try
            {
                string orderId = ReadText("Order ID to cancel: ");
                _orders.CancelOrder(customer, orderId);
                Console.WriteLine("Order cancelled.");
            }
            catch (Exception ex) when (ex is not EndOfInputException)
            {
                ShowError(ex);
            }
        }

        private static void ShowCart(Customer customer)
        {
            Console.WriteLine("Your cart:");
            foreach (CartItem item in customer.Cart.Items)
                Console.WriteLine($"{item.Item.Id} | {item.Item.Name} | {item.Quantity} x {item.Item.GetFinalPrice():0.00} ₾ = {item.Item.GetFinalPrice() * item.Quantity:0.00} ₾");
            decimal subtotal = customer.Cart.TotalPrice;
            Console.WriteLine($"Subtotal: {subtotal:0.00} ₾ | Delivery: {(subtotal > 0 ? 4m : 0m):0.00} ₾ | Total: {subtotal + (subtotal > 0 ? 4m : 0m):0.00} ₾");
        }

        private static void PrintItems(IEnumerable<MenuItem> items)
        {
            foreach (MenuItem item in items)
                Console.WriteLine($"{item.Id} | {item.RestaurantId} | {item.Name} | {item.Category} | {item.GetDetails()} | {item.Price:0.00} ₾");
        }

        private void ShowError(Exception exception) => Console.WriteLine(ErrorHandler.Handle(exception, _logger));

        private static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                int value = ReadInt(prompt);
                if (value > 0) return value;
                Console.WriteLine("Enter a value greater than zero.");
            }
        }

        private static decimal? ReadOptionalDecimal(string prompt)
        {
            while (true)
            {
                string input = ReadText(prompt);
                if (string.IsNullOrWhiteSpace(input)) return null;
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)) return value;
                Console.WriteLine("Enter a valid decimal amount or leave blank.");
            }
        }
    }
}
