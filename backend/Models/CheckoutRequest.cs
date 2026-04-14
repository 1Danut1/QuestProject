namespace backend.Models
{
    public class CheckoutRequest
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public List<CheckoutItemRequest> Items { get; set; } = new();
    }
}