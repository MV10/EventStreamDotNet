
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// A collection to manage and interact with multiple EventStreamManagers for a given domain model. All public
    /// manager methods are reproduced here (except Id and Initialize, which are handled interally), so if multiple
    /// event streams are needed, the client application needs not interact directly with a manager at all.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public class EventStreamCollection<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
    {
        private readonly EventStreamConfigService configService;
        private readonly DomainEventHandlerService eventHandlerService;
        private readonly ProjectionHandlerService projectionHandlerService;

        private readonly EventStreamDotNetConfig config;
        private readonly Dictionary<string, EventStreamManager<TDomainModelRoot>> managers;
        private readonly List<string> fifoQueue;
        private readonly DebugLogger<EventStreamCollection<TDomainModelRoot>> logger;

        private int queueSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configService">A collection of library configuration settings.</param>
        /// <param name="eventHandlerService">A collection of domain event handlers.</param>
        /// <param name="projectionHandlerService">A collection of domain model projection handlers.</param>
        public EventStreamCollection(EventStreamConfigService configService, DomainEventHandlerService eventHandlerService, ProjectionHandlerService projectionHandlerService)
        {
            if (!configService.ContainsConfiguration<TDomainModelRoot>()) throw new Exception($"No configuration registered for domain model {typeof(TDomainModelRoot).Name}");

            this.configService = configService;
            this.eventHandlerService = eventHandlerService;
            this.projectionHandlerService = projectionHandlerService;

            config = configService.GetConfiguration<TDomainModelRoot>();
            queueSize = config.Policies.DefaultCollectionQueueSize;
            managers = new Dictionary<string, EventStreamManager<TDomainModelRoot>>(queueSize + 1);
            fifoQueue = new List<string>(queueSize + 1);
            logger = new DebugLogger<EventStreamCollection<TDomainModelRoot>>(config.LoggerFactory);

            logger.LogDebug($"Created {nameof(EventStreamCollection<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }

        /// <summary>
        /// Constructor for non-DI-based client applications.
        /// </summary>
        /// <param name="serviceHost">An instance of the library's service host.</param>
        public EventStreamCollection(EventStreamServiceHost serviceHost)
            : this(serviceHost.EventStreamConfigs, serviceHost.DomainEventHandlers, serviceHost.ProjectionHandlers)
        { }

        public int QueueSize 
        { 
            get => queueSize; 
            
            set
            {
                queueSize = value;
                TrimQueue();
            }
        }

        public async Task<EventStreamManager<TDomainModelRoot>> GetEventStreamManager(string id) 
        {
            logger.LogDebug($"{nameof(GetEventStreamManager)}({id})");

            if (managers.ContainsKey(id)) return managers[id];

            var mgr = Activator.CreateInstance(typeof(EventStreamManager<TDomainModelRoot>), configService, eventHandlerService, projectionHandlerService) as EventStreamManager<TDomainModelRoot>;
            await mgr.Initialize(id);
            AddManager(mgr);

            return mgr;
        }

        public bool ContainsEventStreamManager(string id)
            => managers.ContainsKey(id);

        public void ReleaseEventStreamManager(string id)
            => managers.Remove(id);

        public List<string> GetEventStreamIds()
            => new List<string>(fifoQueue);

        public async Task<TDomainModelRoot> GetCopyOfState(string id, bool forceRefresh = false)
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.GetCopyOfState(forceRefresh);
        }

        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(string id, DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false) 
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.PostDomainEvent(delta, onlyWhenCurrent, doNotCopyState);
        }

        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(string id, IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false) 
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.PostDomainEvents(deltas, onlyWhenCurrent, doNotCopyState);
        }

        /// <summary>
        /// Adds an event stream manager to the dictionary and enqueues the ID in the FIFO list.
        /// </summary>
        private void AddManager(EventStreamManager<TDomainModelRoot> manager)
        {
            logger.LogDebug($"EventStreamCollection {nameof(AddManager)} for ID {manager.Id}");

            managers.Add(manager.Id, manager);
            fifoQueue.Add(manager.Id);
            TrimQueue();
        }

        /// <summary>
        /// Drops older event stream managers if the dictionary is larger than the defined QueueSize.
        /// </summary>
        private void TrimQueue()
        {
            if (queueSize == 0) return;

            if (queueSize < 0)
                throw new ArgumentException("EventStreamCollection queue size must be 0 or larger");

            logger.LogDebug($"{nameof(TrimQueue)}");

            while(fifoQueue.Count > 0 && managers.Count > queueSize)
            {
                var id = fifoQueue[0];
                fifoQueue.RemoveAt(0);
                managers.Remove(id);
            }
        }
    }
}
