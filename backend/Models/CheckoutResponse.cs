namespace backend.Models
{
    public class CheckoutResponse
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public List<CheckoutItemResponse> Items { get; set; } = new();
        public decimal Total { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}