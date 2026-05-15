using System.Security.Claims;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OrdersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] CheckoutRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ShippingAddress))
            {
                return BadRequest("Shipping address is required.");
            }

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return BadRequest("City is required.");
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest("Phone number is required.");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("At least one cart item is required.");
            }

            int userId = GetAuthenticatedUserId();
            if (userId <= 0)
            {
                return Unauthorized("Invalid authentication context.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var orderItems = new List<OrderItemResponse>();
            decimal total = 0m;
            int orderId;
            DateTime createdAt = DateTime.UtcNow;

            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var orderColumns = GetTableColumns(connection, transaction, "Orders");
                var userIdColumn = ResolveFirstExisting(orderColumns, "UserId", "CustomerId");
                var shippingAddressColumn = ResolveFirstExisting(orderColumns, "ShippingAddress", "Address");
                var cityColumn = ResolveFirstExisting(orderColumns, "City");
                var phoneColumn = ResolveFirstExisting(orderColumns, "PhoneNumber", "Phone");
                var totalColumn = ResolveFirstExisting(orderColumns, "Total", "TotalAmount", "TotalPrice", "GrandTotal");
                var statusColumn = ResolveFirstExisting(orderColumns, "Status");
                var createdAtColumn = ResolveFirstExisting(orderColumns, "CreatedAt", "OrderDate", "CreatedOn");

                if (string.IsNullOrEmpty(userIdColumn) || string.IsNullOrEmpty(shippingAddressColumn))
                {
                    transaction.Rollback();
                    return StatusCode(500, "Orders table schema is missing required user or shipping address columns.");
                }

                foreach (var item in request.Items)
                {
                    if (item.Quantity <= 0)
                    {
                        transaction.Rollback();
                        return BadRequest("Quantity must be greater than 0.");
                    }

                    const string productQuery = "SELECT Id, Name, Price FROM Products WHERE Id = @Id";
                    using SqlCommand productCommand = new(productQuery, connection, transaction);
                    productCommand.Parameters.AddWithValue("@Id", item.ProductId);

                    using SqlDataReader reader = productCommand.ExecuteReader();
                    if (!reader.Read())
                    {
                        transaction.Rollback();
                        return BadRequest($"Product with ID {item.ProductId} was not found.");
                    }

                    decimal unitPrice = reader.GetDecimal(reader.GetOrdinal("Price"));
                    string productName = reader.GetString(reader.GetOrdinal("Name"));
                    decimal lineTotal = unitPrice * item.Quantity;
                    total += lineTotal;

                    orderItems.Add(new OrderItemResponse
                    {
                        ProductId = item.ProductId,
                        ProductName = productName,
                        UnitPrice = unitPrice,
                        Quantity = item.Quantity,
                        LineTotal = lineTotal
                    });
                }

                var insertColumns = new List<string> { userIdColumn, shippingAddressColumn };
                var insertParams = new List<string> { "@UserId", "@ShippingAddress" };
                var orderParameters = new List<SqlParameter>
                {
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@ShippingAddress", request.ShippingAddress),
                };

                if (!string.IsNullOrEmpty(cityColumn))
                {
                    insertColumns.Add(cityColumn);
                    insertParams.Add("@City");
                    orderParameters.Add(new SqlParameter("@City", request.City));
                }

                if (!string.IsNullOrEmpty(phoneColumn))
                {
                    insertColumns.Add(phoneColumn);
                    insertParams.Add("@PhoneNumber");
                    orderParameters.Add(new SqlParameter("@PhoneNumber", request.PhoneNumber));
                }

                if (!string.IsNullOrEmpty(totalColumn))
                {
                    insertColumns.Add(totalColumn);
                    insertParams.Add("@Total");
                    orderParameters.Add(new SqlParameter("@Total", total));
                }

                if (!string.IsNullOrEmpty(statusColumn))
                {
                    insertColumns.Add(statusColumn);
                    insertParams.Add("@Status");
                    orderParameters.Add(new SqlParameter("@Status", "Pending"));
                }

                if (!string.IsNullOrEmpty(createdAtColumn))
                {
                    insertColumns.Add(createdAtColumn);
                    insertParams.Add("@CreatedAt");
                    orderParameters.Add(new SqlParameter("@CreatedAt", createdAt));
                }

                string insertOrderQuery = $@"
                    INSERT INTO Orders ({string.Join(", ", insertColumns)})
                    OUTPUT INSERTED.Id
                    VALUES ({string.Join(", ", insertParams)})";

                using (SqlCommand orderCommand = new(insertOrderQuery, connection, transaction))
                {
                    orderCommand.Parameters.AddRange(orderParameters.ToArray());
                    object? newOrderId = orderCommand.ExecuteScalar();
                    if (newOrderId == null)
                    {
                        transaction.Rollback();
                        return StatusCode(500, "Failed to create order.");
                    }

                    orderId = Convert.ToInt32(newOrderId);
                }

                const string insertItemQuery = @"
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                    VALUES (@OrderId, @ProductId, @Quantity, @Price)";

                foreach (var item in orderItems)
                {
                    using SqlCommand itemCommand = new(insertItemQuery, connection, transaction);
                    itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                    itemCommand.Parameters.AddWithValue("@ProductId", item.ProductId);
                    itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                    itemCommand.Parameters.AddWithValue("@Price", item.UnitPrice);
                    itemCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                return BadRequest($"Order creation failed due to database validation: {ex.Message}");
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return Ok(new CheckoutResponse
            {
                Message = "Order placed successfully.",
                ShippingAddress = request.ShippingAddress,
                Total = total,
                Items = orderItems.Select(item => new CheckoutItemResponse
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    LineTotal = item.LineTotal
                }).ToList()
            });
        }

        [HttpGet("my")]
        public IActionResult GetMyOrders()
        {
            int userId = GetAuthenticatedUserId();
            if (userId <= 0)
            {
                return Unauthorized("Invalid authentication context.");
            }

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var orders = new Dictionary<int, OrderResponse>();

            using SqlConnection connection = new(connectionString);
            connection.Open();
            var orderColumns = GetTableColumns(connection, null, "Orders");
            var userIdColumn = ResolveFirstExisting(orderColumns, "UserId", "CustomerId");
            var shippingAddressColumn = ResolveFirstExisting(orderColumns, "ShippingAddress", "Address");
            var statusColumn = ResolveFirstExisting(orderColumns, "Status");
            var createdAtColumn = ResolveFirstExisting(orderColumns, "CreatedAt", "OrderDate", "CreatedOn");
            var totalColumn = ResolveFirstExisting(orderColumns, "Total", "TotalAmount", "TotalPrice", "GrandTotal");
            var itemPriceColumn = "Price";

            if (string.IsNullOrEmpty(userIdColumn))
            {
                return StatusCode(500, "Orders table schema is missing user column.");
            }

            var createdSelect = string.IsNullOrEmpty(createdAtColumn) ? "GETUTCDATE()" : $"o.{createdAtColumn}";
            var statusSelect = string.IsNullOrEmpty(statusColumn) ? "'Pending'" : $"o.{statusColumn}";
            var shippingSelect = string.IsNullOrEmpty(shippingAddressColumn) ? "''" : $"o.{shippingAddressColumn}";
            var totalSelect = string.IsNullOrEmpty(totalColumn) ? "0" : $"o.{totalColumn}";

            string query = $@"
                SELECT
                    o.Id AS OrderId,
                    {createdSelect} AS CreatedAt,
                    {statusSelect} AS Status,
                    {shippingSelect} AS ShippingAddress,
                    {totalSelect} AS Total,
                    oi.ProductId,
                    p.Name AS ProductName,
                    oi.{itemPriceColumn} AS UnitPrice,
                    oi.Quantity,
                    oi.{itemPriceColumn} * oi.Quantity AS LineTotal
                FROM Orders o
                INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                INNER JOIN Products p ON p.Id = oi.ProductId
                WHERE o.{userIdColumn} = @UserId
                ORDER BY CreatedAt DESC, oi.Id ASC";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int orderId = reader.GetInt32(reader.GetOrdinal("OrderId"));
                if (!orders.TryGetValue(orderId, out var order))
                {
                    order = new OrderResponse
                    {
                        OrderId = orderId,
                        OrderDate = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        Status = reader["Status"]?.ToString() ?? "Pending",
                        ShippingAddress = reader["ShippingAddress"]?.ToString() ?? string.Empty,
                        Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                    };

                    orders[orderId] = order;
                }

                order.Items.Add(new OrderItemResponse
                {
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader["ProductName"]?.ToString() ?? string.Empty,
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    LineTotal = reader.GetDecimal(reader.GetOrdinal("LineTotal")),
                });
            }

            return Ok(orders.Values.ToList());
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private static HashSet<string> GetTableColumns(SqlConnection connection, SqlTransaction? transaction, string tableName)
        {
            const string columnsQuery = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName";

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using SqlCommand command = new(columnsQuery, connection, transaction);
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
