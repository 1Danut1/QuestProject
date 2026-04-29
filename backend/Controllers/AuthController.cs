using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // verifică dacă userul există deja
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

                using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Username", request.Username);

                    int userCount = (int)checkCommand.ExecuteScalar();

                    if (userCount > 0)
                    {
                        return BadRequest("Username already exists.");
                    }
                }

                // inserează user nou
                string insertQuery = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";

                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Username", request.Username);
                    insertCommand.Parameters.AddWithValue("@Password", request.Password);

                    insertCommand.ExecuteNonQuery();
                }
            }

            return Ok(new AuthResponse
            {
                Message = "User registered successfully.",
                Username = request.Username
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", request.Username);
                    command.Parameters.AddWithValue("@Password", request.Password);

                    int userCount = (int)command.ExecuteScalar();

                    if (userCount == 0)
                    {
                        return Unauthorized("Invalid username or password.");
                    }
                }
            }

            return Ok(new AuthResponse
            {
                Message = "Login successful.",
                Username = request.Username
            });
        }
    }
}