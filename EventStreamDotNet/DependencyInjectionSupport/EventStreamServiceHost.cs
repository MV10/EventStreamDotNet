
namespace EventStreamDotNet
{
    /// <summary>
    /// This class holds references to library services that would normally be registered for
    /// dependency injection. It is intended for use by client apps that are not DI-based.
    /// </summary>
    public class EventStreamServiceHost
    {
        public EventStreamServiceHost()
        {
            EventStreamConfigs = new EventStreamConfigService();
            DomainEventHandlers = new DomainEventHandlerService();
            ProjectionHandlers = new ProjectionHandlerService(EventStreamConfigs);
        }

        /// <summary>
        /// Caches configuration data according to the domain data model related to the configuration.
        /// </summary>
        public EventStreamConfigService EventStreamConfigs { get; }

        /// <summary>
        /// Caches instances of domain event handlers, and caches and invokes all of their Apply methods.
        /// </summary>
        public DomainEventHandlerService DomainEventHandlers { get; }

        /// <summary>
        /// Caches instances of projection handlers, and caches and invokes their projection methods. Requires
        /// configuration data for the associated domain data model, configure the <see cref="EventStreamConfigs"/> 
        /// service before you configure this service.
        /// </summary>
        public ProjectionHandlerService ProjectionHandlers { get; }
    }
}
