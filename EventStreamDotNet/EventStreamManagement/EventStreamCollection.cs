
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
    public class EventStreamCollection<TDomainModelRoot> : IEventStreamCollection<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
    {
        private readonly IDomainModelEventHandler<TDomainModelRoot> eventHandler;
        private readonly EventStreamDotNetConfig config;
        private readonly Dictionary<string, IEventStreamManager<TDomainModelRoot>> managers;
        private readonly List<string> fifoQueue;
        private readonly DebugLogger<EventStreamCollection<TDomainModelRoot>> logger;

        private int queueSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration for this event stream.</param>
        /// <param name="eventHandler">An instance of the domain event handler for this domain model.</param>
        public EventStreamCollection(EventStreamDotNetConfig config, IDomainModelEventHandler<TDomainModelRoot> eventHandler)
        {
            if (string.IsNullOrWhiteSpace(config.Database.ConnectionString)
                || string.IsNullOrWhiteSpace(config.Database.EventTableName)
                || string.IsNullOrWhiteSpace(config.Database.SnapshotTableName))
                throw new ArgumentException("Missing one or more required database configuration values");

            this.eventHandler = eventHandler;
            this.config = config;
            queueSize = config.Policies.DefaultCollectionQueueSize;
            managers = new Dictionary<string, IEventStreamManager<TDomainModelRoot>>(queueSize + 1);
            fifoQueue = new List<string>(queueSize + 1);
            
            logger = new DebugLogger<EventStreamCollection<TDomainModelRoot>>(config.LoggerFactory);
            logger.LogDebug($"Created {nameof(EventStreamCollection<TDomainModelRoot>)} for domain model root {typeof(TDomainModelRoot).Name}");
        }

        /// <inheritdoc />
        public int QueueSize 
        { 
            get => queueSize; 
            
            set
            {
                queueSize = value;
                TrimQueue();
            }
        }

        /// <inheritdoc />
        public async Task<IEventStreamManager<TDomainModelRoot>> GetEventStreamManager(string id) 
        {
            logger.LogDebug($"{nameof(GetEventStreamManager)}({id})");

            if (managers.ContainsKey(id)) return managers[id];

            var mgr = Activator.CreateInstance(typeof(EventStreamManager<TDomainModelRoot>), id, config, eventHandler) as IEventStreamManager<TDomainModelRoot>;
            await mgr.Initialize();
            AddManager(mgr);

            return mgr;
        }

        /// <inheritdoc />
        public bool ContainsEventStreamManager(string id)
            => managers.ContainsKey(id);

        /// <inheritdoc />
        public void ReleaseEventStreamManager(string id)
            => managers.Remove(id);

        /// <inheritdoc />
        public List<string> GetEventStreamIds()
            => new List<string>(fifoQueue);

        /// <inheritdoc />
        public async Task<TDomainModelRoot> GetCopyOfState(string id, bool forceRefresh = false)
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.GetCopyOfState(forceRefresh);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(string id, DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false) 
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.PostDomainEvent(delta, onlyWhenCurrent, doNotCopyState);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(string id, IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false) 
        {
            var mgr = await GetEventStreamManager(id);
            return await mgr.PostDomainEvents(deltas, onlyWhenCurrent, doNotCopyState);
        }

        /// <summary>
        /// Adds an event stream manager to the dictionary and enqueues the ID in the FIFO list.
        /// </summary>
        private void AddManager(IEventStreamManager<TDomainModelRoot> manager)
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
