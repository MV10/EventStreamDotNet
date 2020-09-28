
using System;
using System.Collections.Generic;

namespace EventStreamDotNet
{
    /// <summary>
    /// Registry of client application handlers that generate projections based
    /// on domain events or snapshot generation. These are configured through code,
    /// not read from a configuration file. The handler is responsible for casting
    /// the input object to the domain model root type.
    /// </summary>
    public class ProjectionConfig
    {
        /// <summary>
        /// Handlers invoked after the snapshot has been updated.
        /// </summary>
        internal List<Action<IDomainModelRoot>> SnapshotHandlers = new List<Action<IDomainModelRoot>>();

        /// <summary>
        /// Handlers invoked after a given domain event has been applied.
        /// </summary>
        internal List<(Type type, Action<IDomainModelRoot> handler)> DomainEventHandlers = new List<(Type, Action<IDomainModelRoot>)>();

        /// <summary>
        /// Adds a snapshot projection handler.
        /// </summary>
        /// <param name="handler">The handler to invoke after the snapshot has been updated.</param>
        public void AddSnapshotHandler(Action<IDomainModelRoot> handler)
            => SnapshotHandlers.Add(handler);

        /// <summary>
        /// Removes a snapshot projection handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveSnapshotHandler(Action<IDomainModelRoot> handler)
            => SnapshotHandlers.Remove(handler);

        /// <summary>
        /// Adds a domain event projection handler to invoke after a domain event is applied to the domain model.
        /// </summary>
        /// <typeparam name="TDomainEvent">The domain event which triggers the projection.</typeparam>
        /// <param name="handler">The handler to invoke after the domain event has been applied.</param>
        public void AddDomainEventHandler<TDomainEvent>(Action<IDomainModelRoot> handler)
            where TDomainEvent : DomainEventBase
            => DomainEventHandlers.Add((typeof(TDomainEvent), handler));

        /// <summary>
        /// Removes a domain event projection handler.
        /// </summary>
        public void RemoveDomainEventHandler(Action<IDomainModelRoot> handler)
            => DomainEventHandlers.RemoveAll(h => h.handler.Equals(handler));
    }
}
