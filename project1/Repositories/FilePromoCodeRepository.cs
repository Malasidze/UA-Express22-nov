using System.Globalization;
using FoodExpress.Data;
using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>File-backed promo-code repository.</summary>
    public sealed class FilePromoCodeRepository : IPromoCodeRepository
    {
        private readonly FileManager _files;
        private readonly Action<string>? _log;
        private readonly List<PromoCode> _promoCodes = [];

        public FilePromoCodeRepository(FileManager files, Action<string>? log = null)
        {
            _files = files;
            _log = log;
            foreach (string line in files.ReadLines(FileManager.PromoCodesFile))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//", StringComparison.Ordinal)) continue;
                try
                {
                    string[] fields = FileUserRepository.Split(line, 5);
                    if (!bool.TryParse(fields[1], out bool isPercentage)) throw new FormatException("Invalid promo type.");
                    PromoCode promo = new(fields[0], isPercentage, decimal.Parse(fields[2], CultureInfo.InvariantCulture), DateTime.ParseExact(fields[3], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                    if (!string.IsNullOrWhiteSpace(fields[4])) promo.MarkUsed(fields[4]);
                    _promoCodes.Add(promo);
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"Skipped malformed PromoCodes.txt row '{line}': {ex.Message}");
                }
            }
        }

        public PromoCode? GetByCode(string code) => _promoCodes.FirstOrDefault(promo => string.Equals(promo.Code, code, StringComparison.OrdinalIgnoreCase));
        public IReadOnlyList<PromoCode> GetAll() => _promoCodes.ToList();

        public void Save(PromoCode promoCode)
        {
            int index = _promoCodes.FindIndex(existing => string.Equals(existing.Code, promoCode.Code, StringComparison.OrdinalIgnoreCase));
            if (index < 0) _promoCodes.Add(promoCode);
            else _promoCodes[index] = promoCode;
            _files.WriteLines(FileManager.PromoCodesFile,
                new[] { "// Code | IsPercentage | Value | ExpiresAt | OrderID" }
                .Concat(_promoCodes.Select(promo => $"{promo.Code} | {promo.IsPercentage.ToString().ToLowerInvariant()} | {promo.Value.ToString(CultureInfo.InvariantCulture)} | {promo.ExpiresAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)} | {promo.OrderId ?? string.Empty}")));
        }
    }
}
