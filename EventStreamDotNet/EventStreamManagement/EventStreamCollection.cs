
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// A collection to manage and interact with multiple EventStreamManagers for a given domain model.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public class EventStreamCollection<TDomainModelRoot> : IEventStreamCollection<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot<TDomainModelRoot>, new()
    {
        private readonly Dictionary<string, IEventStreamManager<TDomainModelRoot>> managers = new Dictionary<string, IEventStreamManager<TDomainModelRoot>>();
        private readonly List<string> fifoQueue = new List<string>();
        private readonly IDomainModelEventHandler<TDomainModelRoot> eventHandler;
        private readonly EventStreamDotNetConfig config;

        private int queueSize = 10;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">The configuration for this event stream.</param>
        /// <param name="eventHandler">An instance of the domain event handler for this domain model.</param>
        public EventStreamCollection(EventStreamDotNetConfig config, IDomainModelEventHandler<TDomainModelRoot> eventHandler)
        {
            this.eventHandler = eventHandler;
            this.config = config;
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
            if (managers.ContainsKey(id)) return managers[id];

            var mgr = Activator.CreateInstance(typeof(IEventStreamManager<TDomainModelRoot>), id, config, eventHandler) as IEventStreamManager<TDomainModelRoot>;
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

            while(fifoQueue.Count > 0 && managers.Count > queueSize)
            {
                var id = fifoQueue[0];
                fifoQueue.RemoveAt(0);
                managers.Remove(id);
            }
        }
    }
}
