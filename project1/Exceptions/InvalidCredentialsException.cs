namespace FoodExpress.Exceptions
{
    /// <summary>Indicates authentication failure without revealing the failing field.</summary>
    public sealed class InvalidCredentialsException : FoodExpressException
    {
        public InvalidCredentialsException() : base("Invalid username or password.") { }
    }
}
