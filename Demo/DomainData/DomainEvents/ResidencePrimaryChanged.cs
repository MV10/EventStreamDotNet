using EventStreamDotNet;

namespace Demo
{
    public class ResidencePrimaryChanged : DomainEventBase
    {
        public Address Address { get; set; }
    }
}
