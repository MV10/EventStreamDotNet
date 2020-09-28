
using EventStreamDotNet;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EventStreamDotNet demo");

            // The library can optionally be configured with any implementation of the
            // standard Microsoft ILoggerFactory to enable debug-level log output from
            // the library. Here we're using Serilog but Microsoft's own basic logger
            // or other libraries like NLog could be used.
            var loggerFactory = new LoggerFactory()
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .CreateLogger());

            try
            {
                AppConfig.LoadConfiguration();
                Console.WriteLine($"Database: {AppConfig.Get.EventStreamDotNet.Database.ConnectionString}");
                Console.WriteLine($"Log Table: {AppConfig.Get.EventStreamDotNet.Database.EventTableName}");
                Console.WriteLine($"Snapshot Table: {AppConfig.Get.EventStreamDotNet.Database.SnapshotTableName}");

                AppConfig.Get.EventStreamDotNet.LoggerFactory = loggerFactory;

                var customers = new EventStreamCollection<Customer>(AppConfig.Get.EventStreamDotNet, new CustomerEventHandler());

            }
            catch(Exception ex)
            {
                // This is using the Serilog static logger.
                Log.Logger.Error(ex, ex.Message);
            }
            finally
            {
                // Also a Serilog-only concept.
                Log.CloseAndFlush();
            }
        }
    }
}
