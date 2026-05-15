using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using backend.Models;
using backend.Security;

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
                var userColumns = GetTableColumns(connection, "Users");
                var passwordHashColumn = ResolveFirstExisting(userColumns, "PasswordHash");
                var passwordColumn = ResolveFirstExisting(userColumns, "Password");

                if (string.IsNullOrEmpty(passwordHashColumn) && string.IsNullOrEmpty(passwordColumn))
                {
                    return StatusCode(500, "Users table is missing password column.");
                }

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
                string passwordHash = PasswordHasher.ComputeSha256Hex(request.Password);
                string insertQuery;

                using (SqlCommand insertCommand = new SqlCommand())
                {
                    insertCommand.Connection = connection;
                    insertCommand.Parameters.AddWithValue("@Username", request.Username);

                    if (!string.IsNullOrEmpty(passwordHashColumn))
                    {
                        insertQuery = $"INSERT INTO Users (Username, {passwordHashColumn}) VALUES (@Username, @PasswordHash)";
                        insertCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    }
                    else
                    {
                        insertQuery = $"INSERT INTO Users (Username, {passwordColumn}) VALUES (@Username, @Password)";
                        insertCommand.Parameters.AddWithValue("@Password", request.Password);
                    }

                    insertCommand.CommandText = insertQuery;

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
                var userColumns = GetTableColumns(connection, "Users");
                var passwordHashColumn = ResolveFirstExisting(userColumns, "PasswordHash");
                var passwordColumn = ResolveFirstExisting(userColumns, "Password");

                if (string.IsNullOrEmpty(passwordHashColumn) && string.IsNullOrEmpty(passwordColumn))
                {
                    return StatusCode(500, "Users table is missing password column.");
                }

                string selectedPasswordColumn = !string.IsNullOrEmpty(passwordHashColumn) ? passwordHashColumn : passwordColumn!;
                string query = $"SELECT Id, {selectedPasswordColumn} AS StoredPassword FROM Users WHERE Username = @Username";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", request.Username);
                    using SqlDataReader reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return Unauthorized("Invalid username or password.");
                    }

                    userId = reader.GetInt32(reader.GetOrdinal("Id"));
                    string storedPassword = reader["StoredPassword"]?.ToString() ?? string.Empty;

                    bool validLogin;
                    if (!string.IsNullOrEmpty(passwordHashColumn))
                    {
                        string requestHash = PasswordHasher.ComputeSha256Hex(request.Password);
                        validLogin = string.Equals(storedPassword, requestHash, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        validLogin = string.Equals(storedPassword, request.Password, StringComparison.Ordinal);
                    }

                    if (!validLogin)
                    {
                        return Unauthorized("Invalid username or password.");
                    }
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

        private static HashSet<string> GetTableColumns(SqlConnection connection, string tableName)
        {
            const string columnsQuery = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName";

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using SqlCommand command = new(columnsQuery, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader.GetString(0));
            }

            return result;
        }

        private static string? ResolveFirstExisting(HashSet<string> columns, params string[] candidates)
        {
            foreach (var name in candidates)
            {
                if (columns.Contains(name))
                {
                    return name;
                }
            }

            return null;
        }
    }
}