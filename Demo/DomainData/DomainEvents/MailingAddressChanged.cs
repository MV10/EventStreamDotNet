using EventStreamDotNet;

namespace Demo
{
    public class MailingAddressChanged : DomainEventBase
    {
        public Address Address { get; set; }
    }
}
