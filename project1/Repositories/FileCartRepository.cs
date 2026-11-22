using System.Globalization;
using FoodExpress.Data;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed cart repository.</summary>
    public sealed class FileCartRepository : ICartRepository
    {
        private readonly FileManager _files;
        private readonly Action<string>? _log;

        public FileCartRepository(FileManager files, Action<string>? log = null)
        {
            _files = files;
            _log = log;
        }

        public Cart Load(string userId, IMenuRepository menuRepository)
        {
            Cart cart = new();
            foreach (string line in _files.ReadLines(FileManager.CartsFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 3);
                    if (fields[0] != userId) continue;
                    if (!int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int quantity)) throw new FormatException("Invalid cart quantity.");
                    MenuItem? item = menuRepository.GetById(fields[1]);
                    if (item is null) throw new FormatException("Cart references an unknown menu item.");
                    cart.AddItem(item, quantity);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed Carts.txt row '{line}': {ex.Message}");
                }
            }

            return cart;
        }

        public void Save(string userId, Cart cart)
        {
            List<string> rows = _files.ReadLines(FileManager.CartsFile)
                .Where(line => string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal) || !string.Equals(line.Split('|')[0].Trim(), userId, StringComparison.Ordinal))
                .ToList();
            if (rows.Count == 0 || !rows.Any(line => line.TrimStart().StartsWith("//", StringComparison.Ordinal))) rows.Insert(0, "// UserID | ItemID | Quantity");
            rows.AddRange(cart.Items.Select(item => $"{userId} | {item.Item.Id} | {item.Quantity}"));
            _files.WriteLines(FileManager.CartsFile, rows);
        }

        public void Clear(string userId) => Save(userId, new Cart());
    }
}
