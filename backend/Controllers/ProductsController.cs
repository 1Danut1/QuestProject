using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ProductsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = new List<Product>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Id, Name, Price FROM Products";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString() ?? "",
                            Price = Convert.ToDecimal(reader["Price"])
                        });
                    }
                }
            }

            return Ok(products);
        }
    }
}