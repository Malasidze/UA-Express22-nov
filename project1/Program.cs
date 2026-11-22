using FoodExpress.Data;
using FoodExpress.Menu;
using FoodExpress.Repositories;
using FoodExpress.Services;
using Microsoft.Extensions.Configuration;

namespace FoodExpress
{
    internal static class Program
    {
        private static async Task Main()
        {
            LoggerService? logger = null;
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .Build();
                EmailSettings emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()
                    ?? throw new InvalidOperationException("EmailSettings are missing from appsettings.json.");

                FileManager files = new();
                files.EnsureInitialFiles();
                logger = new LoggerService(files);

                IUserRepository users = new FileUserRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));
                IRestaurantRepository restaurants = new FileRestaurantRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));
                IMenuRepository menuRepository = new FileMenuRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));
                ICartRepository cartRepository = new FileCartRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));
                IOrderRepository orderRepository = new FileOrderRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));
                IPromoCodeRepository promoRepository = new FilePromoCodeRepository(files, message => logger.Log("WARN", "DATA_PARSE", message));

                EmailService emailService = new(emailSettings, logger);
                AuthService authService = new(users, logger);
                EmailVerificationService verificationService = new(emailService, users, logger);
                CartService cartService = new(cartRepository, menuRepository);
                PromoCodeService promoService = new(promoRepository);
                OrderService orderService = new(orderRepository, cartRepository, promoService, emailService, logger);
                MenuService menuService = new(menuRepository);
                MenuQueryService queryService = new(menuRepository, orderRepository);
                CustomerMenu customerMenu = new(queryService, menuRepository, restaurants, cartService, orderService, logger);
                AdminMenu adminMenu = new(menuRepository, orderRepository, restaurants, menuService, orderService, queryService, logger);
                MainMenu mainMenu = new(authService, verificationService, users, customerMenu, adminMenu, logger);

                while (true)
                {
                    try
                    {
                        await mainMenu.RunAsync();
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.Log("ERROR", "UNHANDLED_APPLICATION_ERROR", ex.ToString());
                        Console.WriteLine("An unexpected error occurred. The application will return to the main menu.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.Log("ERROR", "STARTUP_ERROR", ex.ToString());
                Console.WriteLine("FoodExpress could not start because of a configuration or data error.");
            }
        }
    }
}
