
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
    public class EventStreamManager<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
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
        /// <param name="configService">A collection of library configuration settings.</param>
        /// <param name="eventHandlerService">A collection of domain event handlers.</param>
        /// <param name="projectionHandlerService">A collection of domain model projection handlers.</param>
        public EventStreamManager(EventStreamConfigService configService, DomainEventHandlerService eventHandlerService, ProjectionHandlerService projectionHandlerService)
        {
            if (!configService.ContainsConfiguration<TDomainModelRoot>()) throw new Exception($"No configuration registered for domain model {typeof(TDomainModelRoot).Name}");

            eventStream = new EventStreamProcessor<TDomainModelRoot>(configService, eventHandlerService, projectionHandlerService);
            logger = new DebugLogger<EventStreamManager<TDomainModelRoot>>(configService.GetConfiguration<TDomainModelRoot>().LoggerFactory);

            logger.LogDebug($"Created {nameof(EventStreamManager<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }

        /// <summary>
        /// Constructor for non-DI-based client applications.
        /// </summary>
        /// <param name="serviceHost">An instance of the library's service host.</param>
        public EventStreamManager(EventStreamServiceHost serviceHost)
            : this(serviceHost.EventStreamConfigs, serviceHost.DomainEventHandlers, serviceHost.ProjectionHandlers)
        { }


        public async Task Initialize(string id)
            => await eventStream.Initialize(id);

        public string Id { get => eventStream.Id; }

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

        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            logger.LogDebug($"{nameof(PostDomainEvent)}({nameof(delta)}: {delta.GetType().Name}, {nameof(onlyWhenCurrent)}: {onlyWhenCurrent}, {nameof(doNotCopyState)}: {doNotCopyState})");

            singleEvent.Clear();
            singleEvent.Add(delta);
            return await PostDomainEvents(singleEvent, onlyWhenCurrent, doNotCopyState);
        }

        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            if(logger.Available)
            {
                logger.LogDebug($"{nameof(PostDomainEvents)}({nameof(deltas)}: {deltas.Count}, {nameof(onlyWhenCurrent)}: {onlyWhenCurrent}, {nameof(doNotCopyState)}: {doNotCopyState})");
                foreach (var d in deltas)
                    logger.LogDebug($"  Posting delta: {d.GetType().Name}");
            }

            if (!eventStream.IsInitialized) throw new Exception("The EventStreamManager has not been initialized");

            var success = await eventStream.WriteEvents(deltas, onlyWhenCurrent);
            var state = doNotCopyState ? null : eventStream.CopyState();

            return (success, state);
        }

    }
}
