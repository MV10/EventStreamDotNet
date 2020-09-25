using System;

namespace Demo
{
    public class Person
    {
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Residence { get; set; }
        public string TaxId { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
