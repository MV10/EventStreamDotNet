using System;

namespace EventStreamDotNet
{
    public abstract class DomainEventBase
    {
        public static readonly long NEW_ETAG = -1;
        
        public string Id { get; set; }

        public long ETag { get; set; } = NEW_ETAG;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    }
}
