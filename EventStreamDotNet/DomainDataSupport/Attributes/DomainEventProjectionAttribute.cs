
using System;

namespace EventStreamDotNet
{
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
