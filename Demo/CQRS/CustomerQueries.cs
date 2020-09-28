
using EventStreamDotNet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// There is nothing special about the query portion of a CQRS service. They're
// just routine database reads -- typically against either the domain model
// snapshot or other projection tables. The principle of "eventual consistency"
// usually applies here -- the data may be stale, and it may change later on.

namespace Demo
{
    public class CustomerQueries
    {
        private readonly DatabaseConfig dbConfig;

        public CustomerQueries()
        {
            this.dbConfig = AppConfig.Get.EventStreamDotNet.Database;
        }

        public async Task<APIResult<Customer>> FindCustomer(string id)
        {
            return null;
        }

        public async Task<APIResult<List<string>>> FindAllCustomerIds()
        {
            return null;
        }

        public async Task<APIResult<bool>> CustomerExists(string id)
        {
            return null;
        }
    }
}
