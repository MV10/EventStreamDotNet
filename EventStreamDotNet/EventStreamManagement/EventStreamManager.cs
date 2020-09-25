
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// An instance of an event stream manager.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    /// <typeparam name="TDomainEventHandler">The class which applies domain events to a domain model.</typeparam>
    public class EventStreamManager<TDomainModelRoot, TDomainEventHandler> : IEventStreamManager<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot
        where TDomainEventHandler : class, IDomainModelEventHandler<TDomainModelRoot>, new()
    {
        private readonly EventStream<TDomainModelRoot, TDomainEventHandler> eventStream;

        private List<DomainEventBase> singleEvent = new List<DomainEventBase>(1);

        public EventStreamManager(string id, EventStreamDotNetConfig config)
        {
            eventStream = new EventStream<TDomainModelRoot, TDomainEventHandler>(id, config);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            eventStream.Initialize();
        }

        /// <inheritdoc />
        public string Id { get => eventStream.Id; }

        /// <inheritdoc />
        public async Task<TDomainModelRoot> GetCopyOfState(bool forceRefresh = false)
        {
            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");

            if (forceRefresh)
            {

            }

            return eventStream.State;
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CurrentState)> PostDomainEvent(DomainEventBase delta, bool onlyWhenCurrent = false)
        {
            singleEvent[0] = delta;
            return await PostDomainEvents(singleEvent, onlyWhenCurrent);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false)
        {
            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");



            return (true, eventStream.State);
        }

    }
}
