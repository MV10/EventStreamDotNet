
using EventStreamDotNet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Demo
{
    public class CustomerCommands
    {
        private readonly IEventStreamCollection<Customer> managers;

        public CustomerCommands(IEventStreamCollection<Customer> eventStreamManagersCollection)
        {
            managers = eventStreamManagersCollection;
        }
    }
}
