
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
            logger = new DebugLogger<EventStreamManager<TDomainModelRoot>>(configService.LoggerFactory);

            logger.LogDebug($"Created {nameof(EventStreamManager<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }

        /// <summary>
        /// Constructor for non-DI-based client applications.
        /// </summary>
        /// <param name="serviceHost">An instance of the library's service host.</param>
        public EventStreamManager(DirectDependencyServiceHost serviceHost)
            : this(serviceHost.EventStreamConfigs, serviceHost.DomainEventHandlers, serviceHost.ProjectionHandlers)
        { }

        /// <summary>
        /// The manager must be initialized with the domain model object's unique ID before
        /// any of the other methods can be used.
        /// </summary>
        /// <param name="id">The unique identifier corresponding to the domain model object.</param>
        public async Task Initialize(string id)
            => await eventStream.Initialize(id);

        /// <summary>
        /// The unique identifier corresponding to this instance of the domain model.
        /// </summary>
        public string Id { get => eventStream.Id; }

        /// <summary>
        /// Returns a copy of the domain model object's state.
        /// </summary>
        /// <param name="forceRefresh">When true, any new domain events in the database will be applied to the manager's copy of the domain model object's state.</param>
        public async Task<TDomainModelRoot> GetCopyOfState(bool forceRefresh = false)
        {
            logger.LogDebug($"{nameof(GetCopyOfState)}({nameof(forceRefresh)}: {forceRefresh})");

            if (!eventStream.IsInitialized) 
                throw new Exception("The EventStreamManager has not been initialized");

            if (forceRefresh)
            {
                await eventStream.ReadAllEvents();
            }

            return eventStream.CopyState();
        }

        /// <summary>
        /// Stores a single domain event and applies it to the manager's copy of the domain model object's
        /// state. Based on the configured policies and settings, this may also update the domain model's
        /// snapshot and invoke projection handlers.
        /// </summary>
        /// <param name="delta">The domain event to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, ensures the last known ETag matches the highest stored ETag. (Some domain events are not
        /// sensitive to this, such as a deposit transaction, while others are, such as a withdrawal that could be denied to avoid an overdraft.)</param>
        /// <param name="doNotCopyState">When true, the CopyOfCurrentState value will be null. May improve performance if the caller doesn't immediately need new model state data.</param>
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            singleEvent.Clear();
            singleEvent.Add(delta);
            return await PostDomainEvents(singleEvent, onlyWhenCurrent, doNotCopyState);
        }

        /// <summary>
        /// Stores a list of domain events and applies them to the manager's copy of the domain model object's
        /// state. Based on the configured policies and settings, this may also update the domain model's
        /// snapshot and invoke projection handlers.
        /// </summary>
        /// <param name="deltas">The domain events to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, ensures the last known ETag matches the highest stored ETag. (Some domain events are not
        /// sensitive to this, such as a deposit transaction, while others are, such as a withdrawal that could be denied to avoid an overdraft.)</param>
        /// <param name="doNotCopyState">When true, the CopyOfCurrentState value will be null. May improve performance if the caller doesn't immediately need new model state data.</param>
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false)
        {
            if(logger.Available)
            {
                logger.LogDebug($"{nameof(PostDomainEvents)}({nameof(deltas)}: {deltas.Count}, {nameof(onlyWhenCurrent)}: {onlyWhenCurrent}, {nameof(doNotCopyState)}: {doNotCopyState})");
                foreach (var d in deltas)
                    logger.LogDebug($"  Posting domain event: {d.GetType().Name}");
            }

            if (!eventStream.IsInitialized) 
                throw new Exception("The EventStreamManager has not been initialized");

            var success = await eventStream.WriteEvents(deltas, onlyWhenCurrent);
            var state = doNotCopyState ? null : eventStream.CopyState();

            return (success, state);
        }
    }
}
