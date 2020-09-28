
namespace EventStreamDotNet
{
    /// <summary>
    /// Defines the root class in a POCO domain data model sourced from an event stream.
    /// </summary>
    public interface IDomainModelRoot
    {
        /// <summary>
        /// A unique identifier assigned to the entier domain data model's event stream.
        /// </summary>
        string Id { get; set; }
    }
}

