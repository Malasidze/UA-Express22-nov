namespace FoodExpress.Models
{
    /// <summary>Defines order lifecycle states.</summary>
    public enum OrderStatus
    {
        Created,
        Confirmed,
        Preparing,
        OnTheWay,
        Delivered,
        Cancelled
    }
}
