
using System;
using System.Collections.Generic;

namespace EventStreamDotNet
{
    /// <summary>
    /// Caches configuration data according to the domain data model related to the configuration. When used
    /// with dependency injection, register this as a Singleton scoped service. To use this without dependency
    /// injection, reference the instance provided by an <see cref="EventStreamServiceHost"/> object.
    /// </summary>
    public class EventStreamConfigService
    {
        // Keyed on the domain model root type
        private Dictionary<Type, EventStreamDotNetConfig> cache = new Dictionary<Type, EventStreamDotNetConfig>();

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
