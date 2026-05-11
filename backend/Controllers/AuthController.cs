using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
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

            int userId;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Id FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", request.Username);
                    command.Parameters.AddWithValue("@Password", request.Password);

                    object? result = command.ExecuteScalar();
                    if (result == null)
                    {
                        return Unauthorized("Invalid username or password.");
                    }

                    userId = Convert.ToInt32(result);
                }
            }

            string token = GenerateToken(userId, request.Username);

            return Ok(new AuthResponse
            {
                Message = "Login successful.",
                Username = request.Username,
                Token = token
            });
        }

        private string GenerateToken(int userId, string username)
        {
            string jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing.");
            string jwtIssuer = _configuration["Jwt:Issuer"] ?? "QuestProject";
            string jwtAudience = _configuration["Jwt:Audience"] ?? "QuestProjectClient";
            int expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out int minutes) ? minutes : 120;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}