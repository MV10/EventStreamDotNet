
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// Defines a collection to manage multiple EventStreamManagers for a given domain model. Useful for clients
    /// based upon interface-based dependency injection.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public interface IEventStreamCollection<TDomainModelRoot>
        where TDomainModelRoot : class, IDomainModelRoot, new()
    {
        /// <summary>
        /// Defines the maximum number of EventStreamManagers that may be enqueued in the collection.
        /// Managers are released in a FIFO sequence (oldest manager is released first). Changing the
        /// queue size to a smaller value instantly releases older managers if the collection holds a
        /// larger number. Zero disables the upper limit.
        /// </summary>
        int QueueSize { get; set; }

        /// <summary>
        /// Retrieves an event stream manager for the given ID. If the manager has not yet been created,
        /// it will be initialized and added to the collection.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        Task<IEventStreamManager<TDomainModelRoot>> GetEventStreamManager(string id);

        /// <summary>
        /// Indicates whether an EventStreamManager with the indicated ID has already been stored
        /// in the collection.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        bool ContainsEventStreamManager(string id);

        /// <summary>
        /// If an EventStreamManager with the indicated ID is stored in the collection, it will be
        /// removed from the collection.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        void ReleaseEventStreamManager(string id);

        /// <summary>
        /// Returns a list of the event stream IDs currently held in the collection.
        /// </summary>
        List<string> GetEventStreamIds();

        /// <summary>
        /// A copy of the domain model's state based on the last time this manager read or updated the stream. The
        /// client application should always consider the copy of the state to be stale as soon as the copy is obtained.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        /// <param name="forceRefresh">When true, the manager will check the database and apply any newer events to the model state. Defaults to false.</param>
        Task<TDomainModelRoot> GetCopyOfState(string id, bool forceRefresh = false);

        /// <summary>
        /// Adds a single new domain event to the stream. The manager's domain model state will be updated upon successfully writing the event, and the defined
        /// snapshot policy and any projection handlers will be invoked.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        /// <param name="delta">The domain event to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, the event will only apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state). Defaults to false.</param>
        /// <param name="doNotCopyState">If true, return value CopyOfCurrentState will be null. Useful for high-throughput scenarios where client app will
        /// retrieve state separately at a later time. Default is false.</param>
        Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(string id, DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false);

        /// <summary>
        /// Adds multiple new domainevents to the stream. The manager's domain model state will be updated upon successfully writing the events, and the defined
        /// snapshot policy and any projection handlers will be invoked.
        /// </summary>
        /// <param name="id">The unique identifier for the requested domain model instance.</param>
        /// <param name="deltas">The domain events to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, the events will only apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state). Defaults to false.</param>
        /// </summary>
        Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(string id, IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false);
    }
}
