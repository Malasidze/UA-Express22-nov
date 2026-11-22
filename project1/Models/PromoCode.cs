using FoodExpress.Exceptions;

namespace FoodExpress.Models
{
    /// <summary>A discount code, reserved for at most one order.</summary>
    public sealed class PromoCode
    {
        public PromoCode(string code, bool isPercentage, decimal value, DateTime expiresAt)
        {
            if (value <= 0 || (isPercentage && value > 100))
            {
                throw new ValidationException("Promo code value is invalid.");
            }

            Code = code;
            IsPercentage = isPercentage;
            Value = value;
            ExpiresAt = expiresAt;
        }

        public string Code { get; }
        public bool IsPercentage { get; }
        public decimal Value { get; }
        public DateTime ExpiresAt { get; }
        public string? OrderId { get; private set; }
        public bool IsUsed => OrderId is not null;

        public void MarkUsed(string orderId)
        {
            if (IsUsed)
            {
                throw new PromoCodeException("This promo code has already been used.");
            }

            OrderId = orderId;
        }
    }
}
