
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
        /// Settings passed into projection handler constructors.
        /// </summary>
        public ProjectionConfig Projection { get; set; } = new ProjectionConfig();
    }
}
