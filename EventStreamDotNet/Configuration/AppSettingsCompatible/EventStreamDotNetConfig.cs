
namespace EventStreamDotNet
{
    /// <summary>
    /// The library's root configuration object.
    /// </summary>
    public partial class EventStreamDotNetConfig
    {
        /// <summary>
        /// How the library connects to and uses the event and snapshot tables.
        /// </summary>
        public DatabaseConfig Database { get; set; } = new DatabaseConfig();

        /// <summary>
        /// Various controls relating to event stream and snapshot handling.
        /// </summary>
        public PoliciesConfig Policies { get; set; } = new PoliciesConfig();
    }

    // The rest of this class is in the Programmatic folder as EventStreamDotNetPartial.
}
