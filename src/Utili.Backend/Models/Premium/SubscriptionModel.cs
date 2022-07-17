namespace Utili.Backend.Models;

public class SubscriptionModel
{
    public string Id { get; internal set; }
    public ulong UserId { get; internal set; }
    public int Slots { get; internal set; }
    public int Status { get; set; }
    public string ExpiresAt { get; set; }
}