namespace FoodExpress.Exceptions
{
    /// <summary>Indicates an unavailable menu item.</summary>
    public sealed class ItemUnavailableException : FoodExpressException
    {
        public ItemUnavailableException(string message) : base(message) { }
    }
}
