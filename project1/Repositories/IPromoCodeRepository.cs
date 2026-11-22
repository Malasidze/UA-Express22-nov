using FoodExpress.Models;

namespace FoodExpress.Repositories
{
    /// <summary>Defines promo-code lookup and persistence operations.</summary>
    public interface IPromoCodeRepository
    {
        PromoCode? GetByCode(string code);
        IReadOnlyList<PromoCode> GetAll();
        void Save(PromoCode promoCode);
    }
}
