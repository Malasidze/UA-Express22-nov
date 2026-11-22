namespace FoodExpress.Exceptions
{
    /// <summary>Indicates a role or ownership authorization failure.</summary>
    public sealed class AuthorizationException : FoodExpressException
    {
        public AuthorizationException(string message = "You are not authorized to perform this action.") : base(message) { }
    }
}
