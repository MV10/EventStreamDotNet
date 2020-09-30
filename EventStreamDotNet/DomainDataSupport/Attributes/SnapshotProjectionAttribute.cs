
using System;

namespace EventStreamDotNet
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SnapshotProjectionAttribute : Attribute
    { }
}
