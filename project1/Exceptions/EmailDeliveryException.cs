namespace FoodExpress.Exceptions
{
    /// <summary>Indicates that email delivery failed.</summary>
    public sealed class EmailDeliveryException : FoodExpressException
    {
        public EmailDeliveryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
