namespace Tutorial6.Controllers;

using Tutorial6.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly string _connectionString;

    public WarehouseController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    // Endpoint using direct SQL
    [HttpPost("AddProduct")]
public async Task<IActionResult> AddProduct([FromBody] ProductWarehouseDto request)
{
    if (request.Amount <= 0)
    {
        return BadRequest("Amount must be greater than zero.");
    }

    using (var connection = new SqlConnection(_connectionString))
    {
        try
        {
            await connection.OpenAsync();

            // Fetch the product's base price
            var priceCheckCmd = new SqlCommand("SELECT Price FROM Product WHERE IdProduct = @ProductId", connection);
            priceCheckCmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            var basePrice = (decimal?)await priceCheckCmd.ExecuteScalarAsync();
            if (basePrice == null)
            {
                return NotFound("Product not found.");
            }

            decimal totalPrice = basePrice.Value * request.Amount;

            // Check if product and warehouse exist using specific methods
            if (!await ProductExists(connection, request.ProductId) ||
                !await WarehouseExists(connection, request.WarehouseId))
            {
                return NotFound("Product or Warehouse not found.");
            }

            // Check if there's an order with the given criteria
            var orderCheckCmd = new SqlCommand(
                "SELECT IdOrder FROM [Order] WHERE IdProduct = @ProductId AND Amount >= @Amount AND CreatedAt < @CreatedAt", connection);
            orderCheckCmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            orderCheckCmd.Parameters.AddWithValue("@Amount", request.Amount);
            orderCheckCmd.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

            var orderId = (int?)await orderCheckCmd.ExecuteScalarAsync();
            if (!orderId.HasValue)
            {
                return BadRequest("No matching order found or order already fulfilled.");
            }

            // Update the FulfilledAt column of the order
            var updateCmd = new SqlCommand("UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @OrderId", connection);
            updateCmd.Parameters.AddWithValue("@OrderId", orderId.Value);
            await updateCmd.ExecuteNonQueryAsync();

            // Insert a record into the Product_Warehouse table
            var insertCmd = new SqlCommand(
                "INSERT INTO Product_Warehouse (IdOrder, IdWarehouse, IdProduct, Amount, Price, CreatedAt) VALUES (@IdOrder, @IdWarehouse, @IdProduct, @Amount, @Price, GETDATE()); SELECT SCOPE_IDENTITY();", connection);
            insertCmd.Parameters.AddWithValue("@IdOrder", orderId.Value);
            insertCmd.Parameters.AddWithValue("@IdWarehouse", request.WarehouseId);
            insertCmd.Parameters.AddWithValue("@IdProduct", request.ProductId);
            insertCmd.Parameters.AddWithValue("@Amount", request.Amount);
            insertCmd.Parameters.AddWithValue("@Price", totalPrice); // Use calculated price

            var insertedId = await insertCmd.ExecuteScalarAsync();

            // Convert decimal to int safely
            int warehouseEntryId = Convert.ToInt32(insertedId);

            return Ok(new { WarehouseEntryId = warehouseEntryId });
        }
        catch (SqlException sqlEx)
        {
            return StatusCode(500, "SQL error: " + sqlEx.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }
}




    // Specific existence checks for each table
    private async Task<bool> ProductExists(SqlConnection connection, int productId)
    {
        var cmd = new SqlCommand("SELECT COUNT(1) FROM [Product] WHERE IdProduct = @ProductId", connection);
        cmd.Parameters.AddWithValue("@ProductId", productId);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    private async Task<bool> OrderExists(SqlConnection connection, int orderId)
    {
        var cmd = new SqlCommand("SELECT COUNT(1) FROM [Order] WHERE IdOrder = @OrderId", connection);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    private async Task<bool> WarehouseExists(SqlConnection connection, int warehouseId)
    {
        var cmd = new SqlCommand("SELECT COUNT(1) FROM [Warehouse] WHERE IdWarehouse = @WarehouseId", connection);
        cmd.Parameters.AddWithValue("@WarehouseId", warehouseId);
        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }
    
    [HttpPost("AddProductUsingSP")]
    public IActionResult AddProductUsingSP([FromBody] ProductWarehouseDto request)
    {
        if (request.Amount <= 0)
        {
            return BadRequest("Amount must be greater than zero.");
        }

        using (var connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                // Ensure the stored procedure name is correct as per your SQL server's procedure
                command.CommandText = "EXECUTE AddProductToWarehouse @IdProduct, @WarehouseId, @Amount, @CreatedAt";

                command.Parameters.AddWithValue("@IdProduct", request.ProductId);
                command.Parameters.AddWithValue("@WarehouseId", request.WarehouseId);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);

                connection.Open();
                var result = command.ExecuteScalar();

                if (result == null)
                {
                    return BadRequest("Failed to add product to warehouse.");
                }

                int warehouseEntryId = Convert.ToInt32(result);
                return Ok(new { WarehouseEntryId = warehouseEntryId });
            }
        }
    }

    
    
}
