
namespace EventStreamDotNet
{
    /// <summary>
    /// Defines a class which extracts projection data from domain model state based on
    /// either domain events or snapshot updates.
    /// </summary>
    /// <typeparam name="TDomainModelRoot">The root class of the domain model.</typeparam>
    public interface IDomainModelProjectionHandler<TDomainModelRoot>
    {
        /// <summary>
        /// A copy of the domain model state is assigned before projection methods are called. It will be
        /// reset to null after all projections are processed. The client application must never read
        /// the model state from this property except to process projection extracts within the handler.
        /// </summary>
        TDomainModelRoot DomainModelState { get; set; }
    }
}
