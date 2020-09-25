using EventStreamDotNet;

namespace Demo
{
    public class CustomerCreated : DomainEventBase
    {
        public Person PrimaryAccountHolder { get; set; }
        public Address MailingAddress { get; set; }
    }
}
