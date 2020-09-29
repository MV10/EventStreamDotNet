
using Microsoft.Extensions.Logging;

namespace EventStreamDotNet
{
    /// <summary>
    /// The library's root configuration object.
    /// </summary>
    public class EventStreamDotNetConfig
    {
        /// <summary>
        /// How the library connects to and uses the event and snapshot tables.
        /// </summary>
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();

        /// <summary>
        /// Various controls relating to event stream and snapshot handling.
        /// </summary>
        public PoliciesConfig Policies { get; set; } = new PoliciesConfig();

        /// <summary>
        /// Registry of client application handlers that generate projections based
        /// on domain events or snapshot generation. These are configured through code,
        /// not read from a configuration file.
        /// </summary>
        public ProjectionConfig ProjectionHandlers { get; set; } = new ProjectionConfig();

        /// <summary>
        /// When set, the library will emit Debug-level log output to the configured logger.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
