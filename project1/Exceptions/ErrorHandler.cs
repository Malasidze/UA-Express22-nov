using FoodExpress.Services;

namespace FoodExpress.Exceptions
{
    /// <summary>Maps exceptions to user-safe messages while recording diagnostic details.</summary>
    public static class ErrorHandler
    {
        public static string Handle(Exception exception, LoggerService? logger = null)
        {
            logger?.Log("ERROR", "APPLICATION_ERROR", exception.ToString());
            return exception is FoodExpressException ? exception.Message : "An unexpected error occurred. Please try again.";
        }
    }
}
