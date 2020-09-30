using System;

namespace EventStreamDotNet
{
    /// <summary>
    /// Base class for a client application's domain event POCOs.
    /// </summary>
    public abstract class DomainEventBase
    {
        public static readonly long ETAG_NOT_ASSIGNED = -1;
        
        /// <summary>
        /// The unique ID associated with the domain model object.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Entity Tag (version number) associated with this domain event.
        /// </summary>
        public long ETag { get; set; } = ETAG_NOT_ASSIGNED;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    }
}
