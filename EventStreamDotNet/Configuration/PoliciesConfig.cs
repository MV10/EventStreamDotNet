﻿
namespace EventStreamDotNet
{
    /// <summary>
    /// Various controls relating to event stream and snapshot handling.
    /// </summary>
    public class PoliciesConfig
    {
        /// <summary>
        /// Defines how the library updates the domain model snapshot stored in the database.
        /// </summary>
        public SnapshotFrequencyEnum SnapshotFrequency { get; set; } = SnapshotFrequencyEnum.AfterAllEvents;

        /// <summary>
        /// Based on the configured frequency, either the number of events before a new snapshot
        /// is generated, or the number of seconds before a new snapshot is generated.
        /// </summary>
        public long SnapshotInterval { get; set; }

        /// <summary>
        /// The default QueueSize for EventStreamCollections. Set zero to disable collection size limits.
        /// </summary>
        public int DefaultCollectionQueueSize { get; set; } = 0;
    }
}
