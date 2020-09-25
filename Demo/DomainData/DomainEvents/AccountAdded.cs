using EventStreamDotNet;

namespace Demo
{
    public class AccountAdded : DomainEventBase
    {
        public Account Account { get; set; }
    }
}
