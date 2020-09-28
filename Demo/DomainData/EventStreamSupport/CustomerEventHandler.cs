using EventStreamDotNet;

namespace Demo
{
    public class CustomerEventHandler : IDomainModelEventHandler<Customer>
    {
        private Customer customer;

        public Customer DomainModelState 
        { 
            get => customer; 
            set => customer = value; 
        }

        public void Apply(StreamInitialized loggedEvent)
        {
            // Do nothing, this represents the default domain model root constructor (and ETag 0)
        }

        public void Apply(AccountAdded loggedEvent)
        {
            var acct = customer.Accounts.Find(a => a.AccountNumber.Equals(loggedEvent.Account.AccountNumber));
            if(acct == null) customer.Accounts.Add(loggedEvent.Account);
        }

        public void Apply(AccountRemoved loggedEvent)
        {
            customer.Accounts.RemoveAll(a => a.AccountNumber.Equals(loggedEvent.AccountNumber));
        }

        public void Apply(CustomerCreated loggedEvent)
        {
            customer.PrimaryAccountHolder = loggedEvent.PrimaryAccountHolder;
            customer.MailingAddress = loggedEvent.MailingAddress;
        }

        public void Apply(MailingAddressChanged loggedEvent)
        {
            customer.MailingAddress = loggedEvent.Address;
        }

        public void Apply(ResidencePrimaryChanged loggedEvent)
        {
            customer.PrimaryAccountHolder.Residence = loggedEvent.Address;
        }

        public void Apply(ResidenceSpouseChanged loggedEvent)
        {
            customer.Spouse.Residence = loggedEvent.Address;
        }

        public void Apply(SpouseChanged loggedEvent)
        {
            customer.Spouse = loggedEvent.Spouse;
        }

        public void Apply(SpouseRemoved loggedEvent)
        {
            customer.Spouse = null;
        }

        public void Apply(TransactionPosted loggedEvent)
        {
            var acct = customer.Accounts.Find(a => a.AccountNumber.Equals(loggedEvent.AccountNumber));
            if (acct != null) acct.Balance = loggedEvent.NewBalance;
        }
    }
}
