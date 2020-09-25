
namespace EventStreamDotNet
{
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }

        public string LogTableName { get; set; }

        public string SnapshotTableName { get; set; }
    }
}
