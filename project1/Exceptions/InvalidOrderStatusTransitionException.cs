namespace FoodExpress.Exceptions
{
    /// <summary>Indicates an invalid order state transition.</summary>
    public sealed class InvalidOrderStatusTransitionException : FoodExpressException
    {
        public InvalidOrderStatusTransitionException(string message) : base(message) { }
    }
}
