namespace NewDatabase.Entities.Base
{
    public class CustomerEntity
    {
        public string CustomerId { get; internal set; }

        protected CustomerEntity(string customerId)
        {
            CustomerId = customerId;
        }

        internal CustomerEntity() { }
    }
}
