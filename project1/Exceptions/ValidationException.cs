namespace FoodExpress.Exceptions
{
    /// <summary>Indicates invalid user input or domain data.</summary>
    public sealed class ValidationException : FoodExpressException
    {
        public ValidationException(string message) : base(message) { }
    }
}
