
using Microsoft.Extensions.Logging;

namespace EventStreamDotNet
{
    public partial class EventStreamDotNetConfig
    {
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
