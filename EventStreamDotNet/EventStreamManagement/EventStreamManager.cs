
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// The public interface which client applications use to interact with an event stream and the domain model state.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    /// <typeparam name="TDomainEventHandler">The class which applies domain events to a domain model.</typeparam>
    public class EventStreamManager<TDomainModelRoot, TDomainEventHandler> : IEventStreamManager<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
        where TDomainEventHandler : class, IDomainModelEventHandler<TDomainModelRoot>, new()
    {
        /// <summary>
        /// Internal handling of domain model state and related event stream database operations.
        /// </summary>
        private readonly EventStreamProcessor<TDomainModelRoot, TDomainEventHandler> eventStream;

        /// <summary>
        /// Allows the one-event PostDomainEvent call to quickly hand-off to the list-based PostDomainEvents.
        /// </summary>
        private List<DomainEventBase> singleEvent = new List<DomainEventBase>(1);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The unique identifier for this domain model object and event stream.</param>
        /// <param name="config">The configuration for this event stream.</param>
        public EventStreamManager(string id, EventStreamDotNetConfig config)
        {
            eventStream = new EventStreamProcessor<TDomainModelRoot, TDomainEventHandler>(id, config);
        }

        /// <inheritdoc />
        public async Task Initialize() 
            => await eventStream.Initialize();

        /// <inheritdoc />
        public string Id { get => eventStream.Id; }

        /// <inheritdoc />
        public async Task<TDomainModelRoot> GetCopyOfState(bool forceRefresh = false)
        {
            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");

            if (forceRefresh)
            {
                await eventStream.ReadAllEvents();
            }

            return eventStream.CopyState();
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            singleEvent[0] = delta;
            return await PostDomainEvents(singleEvent, onlyWhenCurrent, doNotCopyState);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");

            var success = await eventStream.WriteEvents(deltas, onlyWhenCurrent);
            var state = doNotCopyState ? null : eventStream.CopyState();

            return (success, state);
        }

    }
}
