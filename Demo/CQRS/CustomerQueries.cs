
using EventStreamDotNet;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

// There is nothing special about the query portion of a CQRS service. They're
// just routine database reads -- typically against either the domain model
// snapshot or other projection tables. The principle of "eventual consistency"
// usually applies here -- the data may be stale, and it may change later on.

// Demo quality! SQL Injection risks! This demo is not a SQL tutorial.

namespace Demo
{
    public class CustomerQueries
    {
        private readonly DatabaseConfig dbConfig;
        private readonly IEventStreamCollection<Customer> managers;

        public CustomerQueries(IEventStreamCollection<Customer> eventStreamManagersCollection)
        {
            this.dbConfig = AppConfig.Get.EventStreamDotNet.Database;
            managers = eventStreamManagersCollection;
        }

        public async Task<APIResult<Customer>> FindCustomer(string id)
        {
            try
            {
                var customer = await managers.GetCopyOfState(id);
                return new APIResult<Customer>(customer);
            }
            catch(Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(FindCustomer)}");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<List<string>>> FindAllCustomerIds()
        {
            try
            {
                var results = new List<string>();
                using var connection = new SqlConnection(dbConfig.ConnectionString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand($"SELECT DISTINCT [Id] FROM [{dbConfig.EventTableName}];", connection);
                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult);
                if(reader.HasRows)
                {
                    while (await reader.ReadAsync())
                        results.Add(reader.GetString(0));
                }
                await reader.CloseAsync();
                await connection.CloseAsync();
                return new APIResult<List<string>>(results);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(FindAllCustomerIds)}");
                return new APIResult<List<string>>(ex);
            }
        }

        public async Task<APIResult<bool>> CustomerExists(string id)
        {
            try
            {
                using var connection = new SqlConnection(dbConfig.ConnectionString);
                await connection.OpenAsync();
                using var cmd = new SqlCommand($"SELECT COUNT(*) AS [ScalarVal] FROM [{dbConfig.EventTableName}] WHERE [Id]='{id}';", connection);
                var count = (int) await cmd.ExecuteScalarAsync();
                await connection.CloseAsync();
                return new APIResult<bool>(count > 0);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(CustomerExists)}");
                return new APIResult<bool>(ex);
            }
        }
    }
}
