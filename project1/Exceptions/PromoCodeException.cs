namespace FoodExpress.Exceptions
{
    /// <summary>Indicates an invalid or expired promo code.</summary>
    public sealed class PromoCodeException : FoodExpressException
    {
        public PromoCodeException(string message) : base(message) { }
    }
}
