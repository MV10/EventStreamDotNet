
using System;

namespace EventStreamDotNet
{
    /// <summary>
    /// Indicates the projection method should be invoked when the domain model snapshot is updated.
    /// Can be combined with domain event projection attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SnapshotProjectionAttribute : Attribute
    { }
}
