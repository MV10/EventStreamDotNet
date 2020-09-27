
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStreamDotNet
{
    /// <summary>
    /// Defines an EventStreamManager. Useful for clients based upon interface-based dependency injection.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public interface IEventStreamManager<TDomainModelRoot>
        where TDomainModelRoot : class, new()
    {
        /// <summary>
        /// The unique identifier for this domain model instance.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// This <em>must</em> be called after the constructor and before any other methods are invoked, otherwise
        /// an exception is thrown. This gives the manager a chance to load the current snapshot and apply any new
        /// changes so that the initial model state is valid.
        /// </summary>
        Task Initialize();

        /// <summary>
        /// A copy of the domain model's state based on the last time this manager read or updated the stream. The
        /// client application should always consider the copy of the state to be stale as soon as the copy is obtained.
        /// </summary>
        /// <param name="forceRefresh">When true, the manager will check the database and apply any newer events to the model state. Defaults to false.</param>
        Task<TDomainModelRoot> GetCopyOfState(bool forceRefresh = false);

        /// <summary>
        /// Adds a single new domain event to the stream. The manager's domain model state will be updated upon successfully writing the event, and the defined
        /// snapshot policy and any projection handlers will be invoked.
        /// </summary>
        /// <param name="delta">The domain event to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, the event will only apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state). Defaults to false.</param>
        /// <param name="doNotCopyState">If true, return value CopyOfCurrentState will be null. Useful for high-throughput scenarios where client app will
        /// retrieve state separately at a later time. Default is false.</param>
        Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvent(DomainEventBase delta, bool onlyWhenCurrent = false, bool doNotCopyState = false);

        /// <summary>
        /// Adds multiple new domainevents to the stream. The manager's domain model state will be updated upon successfully writing the events, and the defined
        /// snapshot policy and any projection handlers will be invoked.
        /// </summary>
        /// <param name="deltas">The domain events to store and apply.</param>
        /// <param name="onlyWhenCurrent">When true, the events will only apply if the model state is up to date. Some types of events are not sensitive to this
        /// (such as posting a deposit to an account), while others are (such as posting a withdrawl, which may cause an overdraft if approved against a stale
        /// model state). Defaults to false.</param>
        /// </summary>
        Task<(bool Success, TDomainModelRoot CopyOfCurrentState)> PostDomainEvents(IReadOnlyList<DomainEventBase> deltas, bool onlyWhenCurrent = false, bool doNotCopyState = false);
    }
}
