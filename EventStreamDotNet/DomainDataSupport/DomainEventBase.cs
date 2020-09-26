﻿using System;

namespace EventStreamDotNet
{
    public abstract class DomainEventBase
    {
        public static readonly long ETAG_NOT_ASSIGNED = -1;
        
        public string Id { get; set; }

        public long ETag { get; set; } = ETAG_NOT_ASSIGNED;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    }
}
