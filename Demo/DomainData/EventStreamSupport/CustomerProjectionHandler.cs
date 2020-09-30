
using EventStreamDotNet;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Demo
{
    public class CustomerProjectionHandler : IDomainModelProjectionHandler<Customer>
    {
        private readonly ProjectionConfig config;
        
        public CustomerProjectionHandler(ProjectionConfig config)
        {
            this.config = config;
        }

        // improves readability versus the generalized property name
        private Customer customer;
        public Customer DomainModelState
        {
            get => customer;
            set => customer = value;
        }

        [SnapshotProjection]
        public async Task ProjectCustomerResidency()
        {
            // The StreamInitialized event is just new() on the domain model root; the model isn't
            // necessarily fully or correctly populated yet, but it still triggers a snapshot
            // update. You should always guard against null references, but snapshot handlers are
            // particularly sensitive due to this initialization situation.
            var homeState = customer.PrimaryAccountHolder?.Residence?.StateOrProvince ?? string.Empty;
            if (homeState.Length > 2) homeState = homeState.Substring(0, 2);

            Console.WriteLine($"Projecting customer ID {customer.Id} state of residence as {homeState}");

            await InsertOrUpdate(customer.Id, "ResidencyProjection", "State", homeState);
        }

        [DomainEventProjection(typeof(SpouseChanged))]
        [DomainEventProjection(typeof(SpouseRemoved))]
        public async Task ProjectCustomerMaritalStatus()
        {
            var status = customer.Spouse == null ? "SINGLE" : "MARRIED";

            Console.WriteLine($"Projecting customer ID {customer.Id} marital status as {status}");

            await InsertOrUpdate(customer.Id, "MaritalStatusProjection", "Status", status);
        }

        // SQL Injection Alert: Demo-quality code, don't try this at home!
        private async Task InsertOrUpdate(string id, string table, string col, string value)
        {
            using var connection = new SqlConnection(config.ConnectionString);
            await connection.OpenAsync();

            using var query = new SqlCommand($"SELECT COUNT(*) AS [ScalarVal] FROM [{table}] WHERE [Id]='{id}';", connection);
            int count = (int)await query.ExecuteScalarAsync();

            var sql = (count == 0)
                ? $"INSERT INTO [{table}] ([Id],[{col}]) VALUES ('{id}','{value}');"
                : $"UPDATE [{table}] SET [{col}]='{value}' WHERE [Id]='{id}';";

            using var cmd = new SqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();

            await connection.CloseAsync();
        }
    }
}
