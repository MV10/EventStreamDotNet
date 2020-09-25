using EventStreamDotNet;
using System.Collections.Generic;

namespace Demo
{
    public class Customer : IDomainModelRoot
    {
        public string Id { get; set; }

        public string CustomerAccountNumber { get; set; }
        public Person PrimaryAccountHolder { get; set; }
        public Person Spouse { get; set; }
        public Address MailingAddress { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
    }
}
