
namespace EventStreamDotNet
{
    /// <summary>
    /// This class holds references to library services that would normally be registered for
    /// dependency injection. It is intended for use by client apps that are not DI-based.
    /// </summary>
    public class EventStreamServiceHost
    {
        /// <summary>
        /// Caches configuration data according to the domain data model related to the configuration.
        /// </summary>
        public EventStreamConfigService EventStreamConfigs { get; } = new EventStreamConfigService();

        /// <summary>
        /// Caches instances of domain event handlers, and caches and invokes all of their Apply methods.
        /// </summary>
        public DomainEventHandlerService DomainEventHandlers { get; } = new DomainEventHandlerService();
    }
}
