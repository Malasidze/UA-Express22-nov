using FoodExpress.Data;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed restaurant repository with an in-memory cache.</summary>
    public sealed class FileRestaurantRepository : IRestaurantRepository
    {
        private readonly List<Restaurant> _restaurants = [];

        public FileRestaurantRepository(FileManager files, Action<string>? log = null)
        {
            foreach (string line in files.ReadLines(FileManager.RestaurantsFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 3);
                    if (!bool.TryParse(fields[2], out bool isOpen)) throw new FormatException("Invalid restaurant open state.");
                    _restaurants.Add(new Restaurant(fields[0], fields[1], isOpen));
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Skipped malformed Restaurants.txt row '{line}': {ex.Message}");
                }
            }
        }

        public IReadOnlyList<Restaurant> GetAll() => _restaurants.ToList();
        public Restaurant? GetById(string id) => _restaurants.FirstOrDefault(restaurant => restaurant.Id == id);
    }
}
