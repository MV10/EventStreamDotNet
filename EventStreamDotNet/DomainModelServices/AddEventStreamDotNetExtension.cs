
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace EventStreamDotNet
{
    public static class AddEventStreamDotNetExtension
    {
        /// <summary>
        /// Prepares the EventStreamDotNet library services for dependency injection.
        /// </summary>
        /// <param name="services">The DI service collection.</param>
        /// <param name="LoggerFactory">When set, the library will emit Debug-level log output to the configured logger.</param>
        /// <param name="domainModelConfigs">A lambda for registering domain model configurations.</param>
        /// <param name="domainEventHandlers">A lambda for registering domain event handlers.</param>
        /// <param name="projectionHandlers">A lambda for registering projection handlers.</param>
        public static IServiceCollection AddEventStreamDotNet(this IServiceCollection services, 
            ILoggerFactory loggerFactory = null,
            Action<EventStreamConfigService> domainModelConfigs = null,
            Action<DomainEventHandlerService> domainEventHandlers = null,
            Action<ProjectionHandlerService> projectionHandlers = null)
        {
            if(loggerFactory != null)
                loggerFactory.CreateLogger("EventStreamDotNet").LogDebug($"{nameof(AddEventStreamDotNet)} is configuring and registering library services");

            // Create the services
            var svcConfigs = new EventStreamConfigService(loggerFactory);
            var svcEvents = new DomainEventHandlerService(svcConfigs);
            var svcProjs = new ProjectionHandlerService(svcConfigs);

            // Invoke the lambdas
            domainModelConfigs?.Invoke(svcConfigs);
            domainEventHandlers?.Invoke(svcEvents);
            projectionHandlers?.Invoke(svcProjs);

            // Register the services for injection
            services.AddSingleton(svcConfigs);
            services.AddSingleton(svcEvents);
            services.AddSingleton(svcProjs);

            return services;
        }
    }
}
