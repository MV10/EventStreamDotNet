
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace EventStreamDotNet
{
    /// <summary>
    /// Caches configuration data according to the domain data model related to the configuration. When used
    /// with dependency injection, register this as a Singleton scoped service. To use this without dependency
    /// injection, reference the instance provided by an <see cref="DirectDependencyServiceHost"/> object.
    /// </summary>
    public class EventStreamConfigService
    {
        private readonly DebugLogger<EventStreamConfigService> logger;

        // Keyed on the domain model root type
        private Dictionary<Type, EventStreamDotNetConfig> cache = new Dictionary<Type, EventStreamDotNetConfig>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public EventStreamConfigService()
        { }

        /// <summary>
        /// Constructor for non-DI-based client applications.
        /// </summary>
        /// <param name="LoggerFactory">When set, the library will emit Debug-level log output to the configured logger.</param>
        public EventStreamConfigService(ILoggerFactory loggerFactory = null)
        {
            LoggerFactory = loggerFactory;
            logger = new DebugLogger<EventStreamConfigService>(loggerFactory);
            logger.LogDebug($"{nameof(EventStreamConfigService)} is starting");
        }

        /// <summary>
        /// When provided, the library will output debug messages to the configured logger.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Adds a configuration object to the collection.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model to which this configuration applies.</typeparam>
        /// <param name="config">The library configuration.</param>
        public void AddConfiguration<TDomainModelRoot>(EventStreamDotNetConfig config)
            where TDomainModelRoot : class, IDomainModelRoot, new()
        {
            if (string.IsNullOrWhiteSpace(config.Database.ConnectionString)
                || string.IsNullOrWhiteSpace(config.Database.EventTableName)
                || string.IsNullOrWhiteSpace(config.Database.SnapshotTableName))
                throw new ArgumentException("Missing one or more required database configuration values");

            var domainType = typeof(TDomainModelRoot);
            if (cache.ContainsKey(domainType)) return;
            cache.Add(domainType, config);

            logger.LogDebug($"Registering configuration for domain model {domainType.Name}");
        }

        /// <summary>
        /// Retrieves the configuration for the requested domain data model.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model to which this configuration applies.</typeparam>
        public EventStreamDotNetConfig GetConfiguration<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            => cache[typeof(TDomainModelRoot)];

        /// <summary>
        /// Indicates whether the collection holds configuration settings for the requested domain data model.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model to check.</typeparam>
        public bool ContainsConfiguration<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            => cache.ContainsKey(typeof(TDomainModelRoot));

        /// <summary>
        /// Removes the configuration settings relating to the requested domain data model.
        /// </summary>
        /// <typeparam name="TDomainModelRoot">The domain model for which the configuration should be removed.</typeparam>
        public void RemoveConfiguration<TDomainModelRoot>()
            where TDomainModelRoot : class, IDomainModelRoot, new()
            => cache.Remove(typeof(TDomainModelRoot));
    }
}
