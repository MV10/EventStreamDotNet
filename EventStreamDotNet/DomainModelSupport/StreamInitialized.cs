
namespace EventStreamDotNet
{
    /// <summary>
    /// The first domain event written to any event stream (always corresponds to ETag zero).
    /// </summary>
    public class StreamInitialized : DomainEventBase
    { }
}
