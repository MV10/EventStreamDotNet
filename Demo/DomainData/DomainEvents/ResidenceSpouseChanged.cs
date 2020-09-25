using EventStreamDotNet;

namespace Demo
{
    public class ResidenceSpouseChanged : DomainEventBase
    {
        public Address Address { get; set; }
    }
}
