using System;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("EventStreamDotNet demo");

            AppConfig.LoadConfiguration();
            Console.WriteLine($"Database: {AppConfig.Get.EventStreamDotNet.Database.ConnectionString}");
            Console.WriteLine($"Log Table: {AppConfig.Get.EventStreamDotNet.Database.EventTableName}");
            Console.WriteLine($"Snapshot Table: {AppConfig.Get.EventStreamDotNet.Database.SnapshotTableName}");

        }
    }
}
