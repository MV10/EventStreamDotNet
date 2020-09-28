
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// The public interface which client applications use to interact with an event stream and the domain model state.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public class EventStreamManager<TDomainModelRoot> : IEventStreamManager<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot<TDomainModelRoot>, new()
    {
        /// <summary>
        /// Internal handling of domain model state and related event stream database operations.
        /// </summary>
        private readonly EventStreamProcessor<TDomainModelRoot> eventStream;

        /// <summary>
        /// Allows the one-event PostDomainEvent call to quickly hand-off to the list-based PostDomainEvents.
        /// </summary>
        private List<DomainEventBase> singleEvent = new List<DomainEventBase>(1);

        private readonly DebugLogger<EventStreamManager<TDomainModelRoot>> logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The unique identifier for this domain model object and event stream.</param>
        /// <param name="config">The configuration for this event stream.</param>
        /// <param name="eventHandler">An instance of the domain event handler for this domain model.</param>
        public EventStreamManager(string id, EventStreamDotNetConfig config, IDomainModelEventHandler<TDomainModelRoot> eventHandler)
        {
            eventStream = new EventStreamProcessor<TDomainModelRoot>(id, config, eventHandler);

            logger = new DebugLogger<EventStreamManager<TDomainModelRoot>>(config.LoggerFactory);
            logger.LogDebug($"Created {nameof(EventStreamManager<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }

        /// <inheritdoc />
        public async Task Initialize() 
            => await eventStream.Initialize();

        /// <inheritdoc />
        public string Id { get => eventStream.Id; }

        /// <inheritdoc />
        public async Task<TDomainModelRoot> GetCopyOfState(bool forceRefresh = false)
        {
            logger.LogDebug($"{nameof(GetCopyOfState)}({nameof(forceRefresh)}: {forceRefresh})");

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
            logger.LogDebug($"{nameof(PostDomainEvent)}({nameof(delta)}: {delta.GetType().Name}, {nameof(onlyWhenCurrent)}: {onlyWhenCurrent}, {nameof(doNotCopyState)}: {doNotCopyState})");

            singleEvent[0] = delta;
            return await PostDomainEvents(singleEvent, onlyWhenCurrent, doNotCopyState);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            if(logger.Available)
            {
                logger.LogDebug($"{nameof(PostDomainEvents)}({nameof(deltas)}: {deltas.Count}, {nameof(onlyWhenCurrent)}: {onlyWhenCurrent}, {nameof(doNotCopyState)}: {doNotCopyState})");
                foreach (var d in deltas)
                    logger.LogDebug($"  {nameof(deltas)}: {d.GetType().Name}");
            }

            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");

            var success = await eventStream.WriteEvents(deltas, onlyWhenCurrent);
            var state = doNotCopyState ? null : eventStream.CopyState();

            return (success, state);
        }

    }
}
