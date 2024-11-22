using Microsoft.Data.SqlClient;
using profescipta_test.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace profescipta_test.Repository
{
    public class SalesOrderRepo
    {
        private string _connectionString;

        public SalesOrderRepo(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Create Operation
        public async Task<int> AddSalesOrderAsync(SalesOrderModel salesOrder)
        {
            Console.WriteLine(salesOrder.SalesOrder);
            var query = "INSERT INTO [order] (sales_order, order_date, customer,address) VALUES (@SalesOrder, @OrderDate, @Customer,@Address); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@SalesOrder", salesOrder.SalesOrder);
            command.Parameters.AddWithValue("@OrderDate", salesOrder.OrderDate);
            command.Parameters.AddWithValue("@Customer", salesOrder.Customer);
            command.Parameters.AddWithValue("@Address", salesOrder.Address);

            connection.Open();
            var id = command.ExecuteScalar();
            Console.WriteLine(id);
            return (int)id;
            

        }
        public async Task<List<SalesOrderModel>> GetPagedSalesOrdersAsync(int page, int pageSize, string keyword, string orderDateFilter)
        {
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {
                
                orderDateStart = parsedDate.Date; 
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1); 
            }
            var query = @"
                SELECT id, sales_order, order_date, customer
                FROM [order]
                WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd)
                ORDER BY id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var salesOrders = new List<SalesOrderModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
            command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                salesOrders.Add(new SalesOrderModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrder = reader.GetString(1),
                    OrderDate = reader.GetDateTime(2),
                    Customer = reader.GetString(3)
                });
            }

            return salesOrders;
        }

        public async Task<int> GetTotalSalesOrderAsync(int pageSize, string keyword, string orderDateFilter)
        {
            int totalRecords = 0;
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {
                // Set the start of the day (00:00) and the end of the day (23:59)
                orderDateStart = parsedDate.Date; // 00:00
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var countQuery = @"
                    SELECT COUNT(*)
                    FROM [order]
                    WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                    AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                    AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd)";
                using (var command = new SqlCommand(countQuery, connection))
                {
                    command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
                    command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
                    command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);

                    totalRecords = (int)await command.ExecuteScalarAsync();
                }
            }

            return totalRecords;
        }

        // Update Operation
        public async Task<int> UpdateSalesOrderAsync(int id, SalesOrderModel salesOrder)
        {
            var query = "UPDATE [order] SET sales_order = @SalesOrder, order_date = @OrderDate, customer = @Customer WHERE id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@SalesOrder", salesOrder.SalesOrder);
            command.Parameters.AddWithValue("@OrderDate", salesOrder.OrderDate);
            command.Parameters.AddWithValue("@Customer", salesOrder.Customer);

            connection.Open();
            return await command.ExecuteNonQueryAsync();
        }

        // Delete Operation
        public async Task<int> DeleteSalesOrderAsync(int id)
        {
            var query = "DELETE FROM [order] WHERE id = @Id";
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            return await command.ExecuteNonQueryAsync();
        }

        // Read Operation - Get sales order by id
        public async Task<SalesOrderModel> GetSalesOrderByIdsAsync(int id)
        {
            SalesOrderModel salesOrder = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, sales_order, order_date, customer FROM [order] WHERE id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            salesOrder = new SalesOrderModel
                            {
                                Id = reader.GetInt32(0),
                                SalesOrder = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                Customer = reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return salesOrder;
        }

        // Read Operation - Get sales order by sales_order
        public async Task<SalesOrderModel> GetSalesOrderByOrdersAsync(string sales_order)
        {
            SalesOrderModel salesOrder = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, sales_order, order_date, customer FROM [order] WHERE sales_order = @sales_order";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sales_order", sales_order);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            salesOrder = new SalesOrderModel
                            {
                                Id = reader.GetInt32(0),
                                SalesOrder = reader.GetString(1),
                                OrderDate = reader.GetDateTime(2),
                                Customer = reader.GetString(3)
                            };
                        }
                    }
                }
            }

            return salesOrder;
        }

        public async Task<List<SalesOrderModel>> GetSalesOrdersAsync(string keyword, string orderDateFilter)
        {
            DateTime? orderDateStart = null;
            DateTime? orderDateEnd = null;

            if (DateTime.TryParse(orderDateFilter, out var parsedDate))
            {
                // Set the start of the day (00:00) and the end of the day (23:59)
                orderDateStart = parsedDate.Date; // 00:00
                orderDateEnd = parsedDate.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
            }
            var query = @"
                SELECT id, sales_order, order_date, customer
                FROM [order]
                WHERE(@Keyword = '' OR sales_order LIKE @Keyword OR customer LIKE @Keyword)
                AND (@OrderDateStart IS NULL OR order_date >= @OrderDateStart)
                AND (@OrderDateEnd IS NULL OR order_date <= @OrderDateEnd )";

            var salesOrders = new List<SalesOrderModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? "" : "%" + keyword + "%");
            command.Parameters.AddWithValue("@OrderDateStart", (object)orderDateStart ?? DBNull.Value);
            command.Parameters.AddWithValue("@OrderDateEnd", (object)orderDateEnd ?? DBNull.Value);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                salesOrders.Add(new SalesOrderModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrder = reader.GetString(1),
                    OrderDate = reader.GetDateTime(2),
                    Customer = reader.GetString(3)
                });
            }

            return salesOrders;
        }

        // Read Operation - Get all sales orders
        public async Task<List<OrderItemModel>> GetPagedOrderItemAsync(int salesId, int page, int pageSize)
        {

            var query = @"
                SELECT id, sales_order_id,name, qty, price,total
                FROM [order_item]
                WHERE sales_order_id = @SalesId
                ORDER BY id
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var orderItems = new List<OrderItemModel>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            command.Parameters.AddWithValue("@SalesId", salesId);
            connection.Open();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orderItems.Add(new OrderItemModel
                {
                    Id = reader.GetInt32(0),
                    SalesOrderId = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Qty = reader.GetInt32(3),
                    Price = reader.GetInt32(4),
                    Total = reader.GetInt32(5)
                });
            }

            return orderItems;
        }

        public void BulkInsertOrderItem(int salesId,List<OrderItemModel> data)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Build raw SQL command
                var sqlBuilder = new System.Text.StringBuilder();
                sqlBuilder.Append("INSERT INTO [order_item] (sales_order_id, name, qty,price,total) VALUES ");

                // Append all data rows
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    sqlBuilder.Append($"('{salesId}', '{item.Name}', '{item.Qty}','{item.Price}','{item.Total}')");

                    // Add a comma if not the last row
                    if (i < data.Count - 1)
                    {
                        sqlBuilder.Append(",");
                    }
                }

                // Execute the command
                using (var command = new SqlCommand(sqlBuilder.ToString(), connection))
                {
                    command.ExecuteNonQuery();
                }
            }

        }
    }
}

