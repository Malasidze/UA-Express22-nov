using FoodExpress.Data;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed menu repository with an in-memory cache.</summary>
    public sealed class FileMenuRepository : IMenuRepository
    {
        private readonly FileManager _files;
        private readonly Action<string>? _log;
        private readonly List<MenuItem> _items = [];

        public FileMenuRepository(FileManager files, Action<string>? log = null)
        {
            _files = files;
            _log = log;
            Load();
        }

        public IReadOnlyList<MenuItem> GetAll() => _items.ToList();
        public MenuItem? GetById(string id) => _items.FirstOrDefault(item => item.Id == id);

        public void Save(MenuItem item)
        {
            int index = _items.FindIndex(existing => existing.Id == item.Id);
            if (index < 0) _items.Add(item);
            else _items[index] = item;
            Persist();
        }

        public void Delete(string id)
        {
            _items.RemoveAll(item => item.Id == id);
            Persist();
        }

        private void Load()
        {
            foreach (string line in _files.ReadLines(FileManager.MenuItemsFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 8);
                    decimal price = FileUserRepository.ParseMoney(fields[6]);
                    if (!bool.TryParse(fields[7], out bool available)) throw new FormatException("Invalid availability value.");
                    MenuItem item = fields[2].ToLowerInvariant() switch
                    {
                        "pizza" => CreatePizza(fields, price, available),
                        "drink" => new Drink(fields[0], fields[1], fields[3], fields[4], price, fields[5], available),
                        "dessert" => new Dessert(fields[0], fields[1], fields[3], fields[4], price, fields[5], available),
                        _ => new CustomMenuItem(fields[0], fields[1], fields[2], fields[3], fields[4], price, fields[5], available)
                    };
                    _items.Add(item);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed MenuItems.txt row '{line}': {ex.Message}");
                }
            }
        }

        private static Pizza CreatePizza(string[] fields, decimal price, bool available)
        {
            string[] details = fields[5].Split(';');
            if (details.Length != 2) throw new FormatException("Pizza details must contain size and dough.");
            return new Pizza(fields[0], fields[1], fields[3], fields[4], price, details[0], details[1], available);
        }

        private void Persist() => _files.WriteLines(FileManager.MenuItemsFile,
            new[] { "// ItemID | RestaurantID | Type | Name | Category | Details | Price | IsAvailable" }
                .Concat(_items.Select(item => $"{item.Id} | {item.RestaurantId} | {item.TypeName} | {item.Name} | {item.Category} | {item.GetDetails()} | {FileUserRepository.Money(item.Price)} | {item.IsAvailable.ToString().ToLowerInvariant()}")));
    }
}
