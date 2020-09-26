
namespace EventStreamDotNet
{
    /// <summary>
    /// How the library connects to and uses the event and snapshot tables.
    /// </summary>
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }

        public string EventTableName { get; set; }

        public string SnapshotTableName { get; set; }
    }
}
