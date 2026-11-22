namespace FoodExpress.Exceptions
{
    /// <summary>Indicates that an email address is missing or malformed.</summary>
    public sealed class InvalidEmailException : FoodExpressException
    {
        public InvalidEmailException() : base("Enter a valid email address containing the @ symbol.") { }
    }
}
