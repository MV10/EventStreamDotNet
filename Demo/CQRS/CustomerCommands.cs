
using EventStreamDotNet;
using System;
using System.Collections.Generic;
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

        public CustomerCommands(IEventStreamCollection<Customer> eventStreamManagersCollection)
        {
            managers = eventStreamManagersCollection;
        }

        public async Task<APIResult<Customer>> AddAccount(string customerId, Account account)
        {
            return null;
        }

        public async Task<APIResult<Customer>> NewCustomer(string customerId, Person primaryAccountHolder, Address mailingAddress)
        {
            return null;
        }

        public async Task<APIResult<Customer>> RemoveAccount(string customerId, string accountNumber)
        {
            return null;
        }

        public async Task<APIResult<Customer>> UpdateMailingAddress(string customerId, Address newAddress)
        {
            return null;
        }

        public async Task<APIResult<Customer>> UpdatePrimaryResidence(string customerId, Address newAddress)
        {
            return null;
        }

        public async Task<APIResult<Customer>> UpdateSpouseResidence(string customerId, Address newAddress)
        {
            return null;
        }

        public async Task<APIResult<Customer>> UpdateSpouse(string customerId, Person spouse)
        {
            return null;
        }

        public async Task<APIResult<Customer>> PostAccountTransaction(string customerId, string accountNumber, decimal amount)
        {
            return null;
        }
    }
}
