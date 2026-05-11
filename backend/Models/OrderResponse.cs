namespace backend.Models
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
