namespace backend.Models
{
    public class CheckoutRequest
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<CheckoutItemRequest> Items { get; set; } = new();
    }
}