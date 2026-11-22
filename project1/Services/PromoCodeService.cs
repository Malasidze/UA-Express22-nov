using FoodExpress.Exceptions;
using FoodExpress.Models;
using FoodExpress.Repositories;

namespace FoodExpress.Services
{
    /// <summary>Validates and consumes single-use promo codes.</summary>
    public sealed class PromoCodeService
    {
        private readonly IPromoCodeRepository _promos;

        public PromoCodeService(IPromoCodeRepository promos) => _promos = promos;

        public decimal CalculateDiscount(string? code, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(code)) return 0m;
            PromoCode promo = FindValid(code);
            decimal discount = promo.IsPercentage ? subtotal * promo.Value / 100m : promo.Value;
            return Math.Min(subtotal, discount);
        }

        public void MarkUsed(string code, string orderId)
        {
            PromoCode promo = FindValid(code);
            promo.MarkUsed(orderId);
            _promos.Save(promo);
        }

        private PromoCode FindValid(string code)
        {
            PromoCode promo = _promos.GetByCode(code) ?? throw new PromoCodeException("That promo code is invalid.");
            if (promo.ExpiresAt < DateTime.Now) throw new PromoCodeException("That promo code has expired.");
            if (promo.IsUsed) throw new PromoCodeException("That promo code has already been used.");
            return promo;
        }
    }
}
