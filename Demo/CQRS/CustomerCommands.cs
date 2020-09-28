
using EventStreamDotNet;
using Serilog;
using System;
using System.Threading.Tasks;

// The command portion of CQRS is where the event stream library is employed.
// Typically a CQRS service is stateless, but since this demo is a simple
// console program, we maintain state using an EventStreamCollection object.
// Remember that deltas written to the event stream are past-tense -- they
// describe changes that result from a command, not the command itself (which
// may have multiple outcomes, depending on the state of the domain data). In
// a real application, this would probably be a service registered for injection,
// and would itself inject something like the EventStreamCollection.

namespace Demo
{
    public class CustomerCommands
    {
        private readonly IEventStreamCollection<Customer> managers;
        private readonly CustomerQueries queries;

        public CustomerCommands(IEventStreamCollection<Customer> eventStreamManagersCollection, CustomerQueries customerQueries)
        {
            managers = eventStreamManagersCollection;
            queries = customerQueries;
        }

        public async Task<APIResult<Customer>> AddAccount(string customerId, Account account)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId, 
                    new AccountAdded 
                    { 
                        Account = account 
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(AddAccount)}");
                return new APIResult<Customer>(ex);
            }
        }

        // In the CQRS model, it is the responsibility of the higher-level API (not represented in this demo
        // to first ensure the customer ID doesn't already exist. That's a higher-order validation rule.
        // This call assumes it's safe to proceed with the requested operation.
        public async Task<APIResult<Customer>> NewCustomer(string customerId, Person primaryAccountHolder, Address mailingAddress)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId, 
                    new CustomerCreated 
                    {
                        PrimaryAccountHolder = primaryAccountHolder,
                        MailingAddress = mailingAddress
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(NewCustomer)}");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<Customer>> RemoveAccount(string customerId, string accountNumber)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId, 
                    new AccountRemoved
                    { 
                        AccountNumber = accountNumber
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(RemoveAccount)}");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<Customer>> UpdateMailingAddress(string customerId, Address newAddress)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId,
                    new MailingAddressChanged
                    {
                        Address = newAddress
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(UpdateMailingAddress)}");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<Customer>> UpdatePrimaryResidence(string customerId, Address newAddress)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId,
                    new ResidencePrimaryChanged
                    {
                        Address = newAddress
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(UpdatePrimaryResidence)}");
                return new APIResult<Customer>(ex);
            }
        }

        public async Task<APIResult<Customer>> UpdateSpouseResidence(string customerId, Address newAddress)
        {
            try
            {
                var result = await managers.PostDomainEvent(customerId,
                    new ResidenceSpouseChanged
                    {
                        Address = newAddress
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(UpdateSpouseResidence)}");
                return new APIResult<Customer>(ex);
            }
        }

        // This example can generate one of two possible domain events (SpouseRemoved or SpouseChanged)
        public async Task<APIResult<Customer>> UpdateSpouse(string customerId, Person spouse)
        {
            try
            {
                (bool Success, Customer CopyOfCurrentState) result;

                if(spouse == null)
                {
                    result = await managers.PostDomainEvent(customerId, new SpouseRemoved());
                }
                else
                {
                    result = await managers.PostDomainEvent(customerId, 
                        new SpouseChanged
                        {
                            Spouse = spouse
                        });
                }

                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(UpdateSpouse)}");
                return new APIResult<Customer>(ex);
            }
        }

        // This example has four possible results: customer not found, account not found, insufficient funds, or posting the domain event
        public async Task<APIResult<Customer>> PostAccountTransaction(string customerId, string accountNumber, decimal amount)
        {
            try
            {
                if (!(await queries.CustomerExists(customerId)).Success) return new APIResult<Customer>("Customer not found");
                var customer = await managers.GetCopyOfState(customerId, forceRefresh: true);

                var acct = customer.Accounts.Find(a => a.AccountNumber.Equals(accountNumber));
                if (acct == null) return new APIResult<Customer>("Account not found");

                var oldBalance = acct.Balance;
                var newBalance = oldBalance + amount;
                if (amount < 0 && newBalance < 0) return new APIResult<Customer>("Insufficient funds");

                var result = await managers.PostDomainEvent(customerId,
                    new TransactionPosted
                    {
                        AccountNumber = accountNumber,
                        Amount = amount,
                        OldBalance = oldBalance,
                        NewBalance = newBalance
                    });
                return new APIResult<Customer>(result.Success, result.CopyOfCurrentState, "Domain event failed to post");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Exception in {nameof(PostAccountTransaction)}");
                return new APIResult<Customer>(ex);
            }
        }
    }
}
