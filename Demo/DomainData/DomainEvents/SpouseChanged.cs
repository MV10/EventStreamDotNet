using EventStreamDotNet;

namespace Demo
{
    public class SpouseChanged : DomainEventBase
    {
        public Person Spouse { get; set; }
    }
}
