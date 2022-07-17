namespace Utili.Database.Entities;

public class CustomerDetails
{
    public string CustomerId { get; internal set; }
    public ulong UserId { get; set; }

    public CustomerDetails(string customerId)
    {
        CustomerId = customerId;
    }

    internal CustomerDetails() { }
}