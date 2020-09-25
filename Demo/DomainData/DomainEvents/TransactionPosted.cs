using EventStreamDotNet;

namespace Demo
{
    public class TransactionPosted : DomainEventBase
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }
    }
}
