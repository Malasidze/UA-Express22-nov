namespace FoodExpress.Exceptions
{
    /// <summary>Indicates a file persistence or parsing failure.</summary>
    public sealed class DataFileException : FoodExpressException
    {
        public DataFileException(string message, Exception innerException) : base(message, innerException) { }
    }
}
