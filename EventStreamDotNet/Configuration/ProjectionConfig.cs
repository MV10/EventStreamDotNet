using System;
using System.Collections.Generic;
using System.Linq;

namespace EventStreamDotNet
{
    /// <summary>
    /// Registry of client application handlers that generate projections based
    /// on domain events or snapshot generation. These are configured through code,
    /// not read from a configuration file.
    /// </summary>
    public class ProjectionConfig
    {
        /// <summary>
        /// Handlers invoked after the snapshot has been updated.
        /// </summary>
        internal List<Action> SnapshotHandlers = new List<Action>();

        /// <summary>
        /// Handlers invoked after a given domain event has been applied.
        /// </summary>
        internal List<(Type type, Action handler)> DomainEventHandlers = new List<(Type, Action)>();

        /// <summary>
        /// Adds a snapshot projection handler.
        /// </summary>
        /// <param name="handler">The handler to invoke after the snapshot has been updated.</param>
        public void AddSnapshotHandler(Action handler)
            => SnapshotHandlers.Add(handler);

        /// <summary>
        /// Removes a snapshot projection handler.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveSnapshotHandler(Action handler)
            => SnapshotHandlers.Remove(handler);

        /// <summary>
        /// Adds a domain event projection handler to invoke after a domain event is applied to the domain model.
        /// </summary>
        /// <typeparam name="TDomainEvent">The domain event which triggers the projection.</typeparam>
        /// <param name="handler">The handler to invoke after the domain event has been applied.</param>
        public void AddDomainEventHandler<TDomainEvent>(Action handler)
            where TDomainEvent : DomainEventBase
            => DomainEventHandlers.Add((typeof(TDomainEvent), handler));

        /// <summary>
        /// Removes a domain event projection handler.
        /// </summary>
        public void RemoveDomainEventHandler(Action handler)
            => DomainEventHandlers.RemoveAll(h => h.handler.Equals(handler));
    }
}
