namespace Api.Domain
{
    public class Company : IIndexable
    {
        public string Id { get; set; }
        public string Tenant { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
