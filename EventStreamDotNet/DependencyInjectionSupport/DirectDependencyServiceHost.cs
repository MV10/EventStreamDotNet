
using Microsoft.Extensions.Logging;
using System;

namespace EventStreamDotNet
{
    /// <summary>
    /// This class holds references to library services that would normally be registered for
    /// dependency injection. It is intended for use by client apps that are not DI-based.
    /// </summary>
    public class DirectDependencyServiceHost
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="LoggerFactory">When set, the library will emit Debug-level log output to the configured logger.</param>
        public DirectDependencyServiceHost(ILoggerFactory loggerFactory = null)
        {
            EventStreamConfigs = new EventStreamConfigService(loggerFactory);
            DomainEventHandlers = new DomainEventHandlerService(EventStreamConfigs);
            ProjectionHandlers = new ProjectionHandlerService(EventStreamConfigs);
        }

        /// <summary>
        /// Caches configuration data according to the domain data model related to the configuration.
        /// </summary>
        public EventStreamConfigService EventStreamConfigs { get; }

        /// <summary>
        /// Caches instances of domain event handlers, and caches and invokes all of their Apply methods. Requires
        /// configuration data, configure the <see cref="EventStreamConfigs"/> service before you configure this service.
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
