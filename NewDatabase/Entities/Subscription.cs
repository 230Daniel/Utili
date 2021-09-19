using System;

namespace NewDatabase.Entities
{
    public class Subscription
    {
        public string Id { get; internal set; }
        public ulong UserId { get; set; }
        public int Slots { get; set; }
        public SubscriptionStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }

        public Subscription(string id)
        {
            Id = id;
        }

        internal Subscription() { }

        public bool IsValid()
            => Status is SubscriptionStatus.Active or SubscriptionStatus.PastDue or SubscriptionStatus.Trialing;
    }

    public enum SubscriptionStatus
    {
        Active = 0,            // Subscription is active                                           Valid
        PastDue = 1,           // Subscription renewal was unsuccessful                            Valid
        Unpaid = 2,            // Subscription renewal was unsuccessful for 1 week                 Invalid
        Canceled = 3,          // Subscription has been canceled                                   Invalid
        Incomplete = 4,        // Subscription's initial payment failed                            Invalid
        IncompleteExpired = 5, // Subscription's initial payment has failed for 23 hours           Invalid
        Trialing = 6           // Subscription is a trial period                                   Valid
    }
}
