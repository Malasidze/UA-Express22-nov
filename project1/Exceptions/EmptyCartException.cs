namespace FoodExpress.Exceptions
{
    /// <summary>Indicates that an operation requires a non-empty cart.</summary>
    public sealed class EmptyCartException : FoodExpressException
    {
        public EmptyCartException() : base("Your cart is empty.") { }
    }
}
