using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CheckoutController(IConfiguration configuration)
        {
            _configuration = configuration;
        }  

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] CheckoutRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            {
                return BadRequest("Shipping address is required.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("At least one cart item is required.");
            }

            foreach (var item in request.Items)
            {
                if (item.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than 0.");
                }
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var responseItems = new List<CheckoutItemResponse>();
            decimal total = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var item in request.Items)
                {
                    string query = "SELECT Id, Name, Price FROM Products WHERE Id = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", item.ProductId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return BadRequest($"Product with ID {item.ProductId} was not found.");
                            }

                            decimal unitPrice = reader.GetDecimal(reader.GetOrdinal("Price"));
                            string productName = reader.GetString(reader.GetOrdinal("Name"));

                            decimal lineTotal = unitPrice * item.Quantity;
                            total += lineTotal;

                            responseItems.Add(new CheckoutItemResponse
                            {
                                ProductId = item.ProductId,
                                ProductName = productName,
                                UnitPrice = unitPrice,
                                Quantity = item.Quantity,
                                LineTotal = lineTotal
                            });
                        }
                    }
                }
            }

            var response = new CheckoutResponse
            {
                ShippingAddress = request.ShippingAddress,
                Items = responseItems,
                Total = total,
                Message = "Order placed successfully."
            };

            return Ok(response);
        }
    }
}