using System.Text;
using FoodExpress.Exceptions;

namespace FoodExpress.Data
{
    /// <summary>Creates data files and provides guarded, atomic text-file operations.</summary>
    public sealed class FileManager
    {
        public const string MenuItemsFile = "MenuItems.txt";
        public const string OrdersFile = "Orders.txt";
        public const string OrderItemsFile = "OrderItems.txt";
        public const string UsersFile = "Users.txt";
        public const string CartsFile = "Carts.txt";
        public const string RestaurantsFile = "Restaurants.txt";
        public const string PromoCodesFile = "PromoCodes.txt";
        public const string EmailVerificationFile = "UnverifiedUsers.txt";
        public const string LogsFile = "Logs.txt";

        private readonly string _directory;
        private readonly Action<string>? _logError;

        public FileManager(string? directory = null, Action<string>? logError = null)
        {
            _directory = directory ?? Path.Combine(AppContext.BaseDirectory, "data");
            _logError = logError;
        }

        public string GetPath(string fileName) => Path.Combine(_directory, fileName);

        public void EnsureInitialFiles()
        {
            try
            {
                Directory.CreateDirectory(_directory);
                EnsureFile(MenuItemsFile, "// ItemID | RestaurantID | Type | Name | Category | Details | Price | IsAvailable\nP01 | R01 | Pizza | Margherita | Pizza | Large;Thin | 24.00 ₾ | true\nD01 | R01 | Drink | Lemonade | Drink | 500ml | 5.50 ₾ | true\n");
                EnsureFile(OrdersFile, "// OrderID | UserID | CreatedAt | Address | Subtotal | DeliveryFee | Discount | Total | Status\nO901 | 1001 | 2026-07-27 18:40 | Tbilisi, Chavchavadze 12 | 29.50 ₾ | 4.00 ₾ | 0.00 ₾ | 33.50 ₾ | Confirmed\n");
                EnsureFile(OrderItemsFile, "// OrderID | ItemID | Name | Quantity | UnitPrice | LineTotal\nO901 | P01 | Margherita | 1 | 24.00 ₾ | 24.00 ₾\nO901 | D01 | Lemonade | 1 | 5.50 ₾ | 5.50 ₾\n");
                EnsureFile(UsersFile, "// ID | Username | PasswordHash | Email | Role\n1001 | ana_food | $2a$11$4sSc0vrOkbgkYnr0vVuobep1fs7QmJfKSrv7T3UZMudhth1UsZsiO | ana@example.com | customer\n9001 | food_admin | $2a$11$VA7kUT8CeyG6kHrBWRlX2OU.P6qeKgaskZWY98uf6b5DBOrRiR8SO | admin@foodexpress.ge | admin\n");
                EnsureFile(CartsFile, "// UserID | ItemID | Quantity\n");
                EnsureFile(RestaurantsFile, "// RestaurantID | Name | IsOpen\nR01 | FoodExpress Kitchen | true\n");
                EnsureFile(PromoCodesFile, "// Code | IsPercentage | Value | ExpiresAt | OrderID\nWELCOME10 | true | 10 | 2027-12-31 23:59 | \n");
                EnsureFile(EmailVerificationFile, "// UserID\n");
                EnsureFile(LogsFile, string.Empty);
            }
            catch (Exception ex)
            {
                _logError?.Invoke($"Could not initialize data files: {ex}");
                throw new DataFileException("Could not initialize FoodExpress data files.", ex);
            }
        }

        public string[] ReadLines(string fileName)
        {
            try
            {
                return File.ReadAllLines(GetPath(fileName));
            }
            catch (Exception ex)
            {
                _logError?.Invoke($"Could not read {fileName}: {ex}");
                throw new DataFileException($"Could not read data file {fileName}.", ex);
            }
        }

        public void WriteLines(string fileName, IEnumerable<string> lines)
        {
            string path = GetPath(fileName);
            string tempPath = path + ".tmp";
            try
            {
                Directory.CreateDirectory(_directory);
                File.WriteAllLines(tempPath, lines, new UTF8Encoding(false));
                File.Move(tempPath, path, true);
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch (Exception cleanupException)
                {
                    _logError?.Invoke($"Could not clean temporary file {tempPath}: {cleanupException}");
                }

                _logError?.Invoke($"Could not write {fileName}: {ex}");
                throw new DataFileException($"Could not write data file {fileName}.", ex);
            }
        }

        public void AppendLine(string fileName, string line)
        {
            try
            {
                File.AppendAllText(GetPath(fileName), line + Environment.NewLine, new UTF8Encoding(false));
            }
            catch (Exception ex)
            {
                _logError?.Invoke($"Could not append to {fileName}: {ex}");
                throw new DataFileException($"Could not write data file {fileName}.", ex);
            }
        }

        private void EnsureFile(string fileName, string content)
        {
            if (!File.Exists(GetPath(fileName)))
            {
                File.WriteAllText(GetPath(fileName), content, new UTF8Encoding(false));
            }
        }
    }
}
