
using EventStreamDotNet;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

// These probably wouldn't be implemented in one giant class in a production application,
// and like the CQRS classes, handling things like configuration is demo-quality in this
// example. Since these are effectively event handlers, the async void pattern is valid.
// This requires the methods to very carefully guard against uncaught exceptions, which
// will immediately end the application.

namespace Demo
{
    public class CustomerProjections
    {
        private readonly DatabaseConfig dbConfig;

        public CustomerProjections()
        {
            this.dbConfig = AppConfig.Get.EventStreamDotNet.Database;
        }

        public async void ProjectCustomerResidency(IDomainModelRoot snapshot)
        {
            try
            {
                var customer = snapshot as Customer;
                var homeState = customer.PrimaryAccountHolder.Residence.StateOrProvince;
                if (homeState.Length > 2) homeState = homeState.Substring(0, 2);

                Console.WriteLine($"Projecting customer ID {customer.Id} state of residence as {homeState}");

                await InsertOrUpdate(customer.Id, "ResidencyProjection", "State", homeState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in {nameof(ProjectCustomerResidency)}:\n{ex.Message}");
            }
        }

        public async void ProjectCustomerMaritalStatus(IDomainModelRoot snapshot)
        {
            try
            {
                var customer = snapshot as Customer;
                var status = customer.Spouse == null ? "SINGLE" : "MARRIED";

                Console.WriteLine($"Projecting customer ID {customer.Id} marital status as {status}");

                await InsertOrUpdate(customer.Id, "MaritalStatusProjection", "Status", status);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in {nameof(ProjectCustomerMaritalStatus)}:\n{ex.Message}");
            }
        }

        // SQL Injection Alert: Demo-quality code, don't try this at home!
        private async Task InsertOrUpdate(string id, string table, string col, string value)
        {
            using var connection = new SqlConnection(dbConfig.ConnectionString);

            using var query = new SqlCommand($"SELECT COUNT(*) AS [ScalarVal] FROM [{table}] WHERE [Id]='{id}';", connection);
            int count = (int) await query.ExecuteScalarAsync();

            var sql = (count == 0) 
                ? $"INSERT INTO [{table}] ([Id],[{col}]) VALUES ('{id}','{value}');"
                : $"UPDATE [{table}] SET [{col}]='{value}' WHERE [Id]='{id}';";

            using var cmd = new SqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }
    }
}
