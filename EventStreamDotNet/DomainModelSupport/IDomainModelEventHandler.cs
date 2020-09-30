
namespace EventStreamDotNet
{
    /// <summary>
    /// Defines a class which knows how to apply domain events to a domain model's state.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model for this event stream.</typeparam>
    public interface IDomainModelEventHandler<TDomainModelRoot>
    {
        /// <summary>
        /// A copy of the domain model state is assigned before Apply methods are called. It will be
        /// reset to null after all extant events are processed. The client application must never read
        /// the model state from this property except to apply events within the event handler.
        /// </summary>
        TDomainModelRoot DomainModelState { get; set; }

        /// <summary>
        /// Apply overloads must exist for every domain event defined for the domain model. The state
        /// in <see cref="DomainModelState"/> must be updated according to the content of the event instance.
        /// Every event stream begins with a <see cref="StreamInitialized"/> event which represents the domain
        /// model root's default constructor and the event stream ETag 0. Typically the event handler does nothing
        /// with this event.
        /// </summary>
        /// <param name="loggedEvent">The properties of the logged event, if any.</param>
        void Apply(StreamInitialized loggedEvent);
    }
}
