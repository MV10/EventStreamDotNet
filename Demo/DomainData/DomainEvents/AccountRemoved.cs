using EventStreamDotNet;

namespace Demo
{
    public class AccountRemoved : DomainEventBase
    {
        public string AccountNumber { get; set; }
    }
}
