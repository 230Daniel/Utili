using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class CustomerDetails : CustomerEntity
    {
        public ulong UserId { get; set; }

        public CustomerDetails(string customerId) : base(customerId) { }
    }
}
