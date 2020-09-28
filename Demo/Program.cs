
using EventStreamDotNet;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("\nEventStreamDotNet demo\n");

            // The library can optionally be configured with any implementation of the
            // standard Microsoft ILoggerFactory to enable debug-level log output from
            // the library. Here we're using Serilog but Microsoft's own basic logger
            // or other libraries like NLog could be used.
            var loggerFactory = new LoggerFactory()
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger());

            try
            {
                // Populate the EventStreamDotNet configuration classes from appsettings.json.
                AppConfig.LoadConfiguration();

                Console.WriteLine($"Database: {AppConfig.Get.EventStreamDotNet.Database.ConnectionString}");
                Console.WriteLine($"Log Table: {AppConfig.Get.EventStreamDotNet.Database.EventTableName}");
                Console.WriteLine($"Snapshot Table: {AppConfig.Get.EventStreamDotNet.Database.SnapshotTableName}");

                // This can generate significant amounts of log output, only enable it
                // if you really need to trace through the library calls and arguments.
                AppConfig.Get.EventStreamDotNet.LoggerFactory = loggerFactory;

                Console.Write("\nDelete all records from the demo database (Y/N)? ");
                var key = Console.ReadKey(true);

                if(key.Key.Equals(ConsoleKey.Y))
                {
                    Console.WriteLine("YES\n");
                    await ResetDatabase();
                }
                else
                {
                    Console.WriteLine("NO\n");
                }

                // This is just one possible usage pattern. The thinking here is that a CQRS service will
                // execute commands against a collection of event streams tied to the same domain model, and
                // will execute queries against snapshots and projections for that domain model. Probably a
                // production app would not start a CQRS query service with the EventStreamDotNet configuration,
                // however (it does not, for example, specify projection table names, which might even be stored
                // into a completely separate database for performance reasons).
                var customerManagers = new EventStreamCollection<Customer>(AppConfig.Get.EventStreamDotNet, new CustomerEventHandler());
                var customerQueries = new CustomerQueries(customerManagers);
                var customerCommands = new CustomerCommands(customerManagers, customerQueries);

                // See comments in the CustomerProjections class for more implementation tips and considerations.
                var projections = new CustomerProjections();
                var handlers = AppConfig.Get.EventStreamDotNet.ProjectionHandlers;
                handlers.AddDomainEventHandler<SpouseChanged>(projections.ProjectCustomerMaritalStatus);
                handlers.AddDomainEventHandler<SpouseRemoved>(projections.ProjectCustomerMaritalStatus);
                handlers.AddSnapshotHandler(projections.ProjectCustomerResidency);

                var customerId = "12345678";

                // This is a simple select against the event stream to check whether the ID has ever been used.
                var customerExists = await customerQueries.CustomerExists(customerId);
                Console.WriteLine($"Customer id {customerId} exists? {customerExists.Output}");

                // When queries and commands succeed, the response is usually an updated snapshot of the dmain data.
                APIResult<Customer> result;

                // Create or read the customer record
                if(!customerExists.Output)
                {
                    var residence = new Address
                    {
                        Street = "10 Main St.",
                        City = "Anytown",
                        StateOrProvince = "TX",
                        PostalCode = "90210",
                        Country = "USA"
                    };

                    var person = new Person
                    {
                        FullName = "John Doe",
                        FirstName = "John",
                        LastName = "Doe",
                        Residence = residence,
                        TaxId = "555-55-1234",
                        DateOfBirth = DateTimeOffset.Parse("05/01/1960")
                    };

                    Console.WriteLine("Creating new customer.");
                    result = await customerCommands.NewCustomer(customerId, person, residence);
                    if (!result.Success)
                        throw new Exception($"Failed to create new customer: {result.Message}");
                }
                else
                {
                    Console.WriteLine("Retrieving customer.");
                    result = await customerQueries.FindCustomer(customerId);
                    if (!result.Success)
                        throw new Exception($"Failed to retrieve customer snapshot: {result.Message}");
                }

                // Add or remove a spouse
                var customer = result.Output;
                if(customer.Spouse == null)
                {
                    var spouse = new Person
                    {
                        FullName = "Jane Doe",
                        FirstName = "Jane",
                        LastName = "Doe",
                        Residence = customer.PrimaryAccountHolder.Residence,
                        TaxId = "333-99-4321",
                        DateOfBirth = DateTimeOffset.Parse("11/01/1960")
                    };

                    Console.WriteLine("Adding spouse (yay).");
                    result = await customerCommands.UpdateSpouse(customerId, spouse);
                    if (!result.Success)
                        throw new Exception($"Failed to add spouse: {result.Message}");
                }
                else
                {
                    Console.WriteLine("Removing spouse (boo).");
                    result = await customerCommands.UpdateSpouse(customerId, null);
                    if (!result.Success)
                        throw new Exception($"Failed to remove spouse: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nException caught in Program.Main:\n{ex.Message}");
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
            }
            finally
            {
                // Purge any pending log entries and dispose.
                Log.CloseAndFlush();
            }

            if (!Debugger.IsAttached)
            {
                Console.WriteLine("\n\nPress any key to exit...");
                Console.ReadKey(true);
            }
        }

        static async Task ResetDatabase()
        {
            var cfg = AppConfig.Get.EventStreamDotNet.Database;
            using var connection = new SqlConnection(cfg.ConnectionString);
            await connection.OpenAsync();
            using var cmd1 = new SqlCommand($"TRUNCATE TABLE {cfg.EventTableName};", connection);
            using var cmd2 = new SqlCommand($"TRUNCATE TABLE {cfg.SnapshotTableName};", connection);
            await cmd1.ExecuteNonQueryAsync();
            await cmd2.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }

    }
}
