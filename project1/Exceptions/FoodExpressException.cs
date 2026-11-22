namespace FoodExpress.Exceptions
{
    /// <summary>Base exception for expected FoodExpress application errors.</summary>
    public class FoodExpressException : Exception
    {
        public FoodExpressException(string message) : base(message) { }
        public FoodExpressException(string message, Exception innerException) : base(message, innerException) { }
    }
}
