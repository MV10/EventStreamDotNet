
using System;

namespace EventStreamDotNet
{
    /// <summary>
    /// Indicates the projection method should be invoked when the indicated domain event is stored 
    /// and applied. Multiple domain events can be identified for a single method. Can be combined
    /// with the snapshot projection attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DomainEventProjectionAttribute : Attribute
    {
        public DomainEventProjectionAttribute(Type domainEvent)
        {
            // Attributes should never throw exceptions since they are metadata, and the way
            // the CLR invokes the class will make it difficult to identify the source of the
            // exception. Instead we'll just store a fake event.

            if (!typeof(DomainEventBase).IsAssignableFrom(domainEvent))
            {
                DomainEvent = typeof(InvalidDomainEventProjectionType);
            }
            else
            {
                DomainEvent = domainEvent;
            }
        }

        public Type DomainEvent { get; }

        private class InvalidDomainEventProjectionType : DomainEventBase
        { }
    }

}
