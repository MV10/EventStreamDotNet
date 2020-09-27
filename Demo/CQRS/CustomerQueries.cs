
using EventStreamDotNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo
{
    public class CustomerQueries
    {
        private readonly DatabaseConfig eventStreamDatabaseConfig;

        public CustomerQueries(DatabaseConfig eventStreamDatabaseConfig)
        {
            this.eventStreamDatabaseConfig = eventStreamDatabaseConfig;
        }

        public async Task<APIResult<Customer>> FindCustomer(string id)
        {

        }

        public async Task<APIResult<List<string>>> FindAllCustomerIds()
        {

        }

        public async Task<APIResult<bool>> CustomerExists(string id)
        {

        }
    }
}
