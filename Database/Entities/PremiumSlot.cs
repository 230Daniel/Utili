namespace Database.Entities
{
    public class PremiumSlot
    {
        public int SlotId { get; internal set; }
        public ulong UserId { get; internal set; }
        public ulong GuildId { get; set; }

        public PremiumSlot(ulong userId)
        {
            UserId = userId;
        }

        internal PremiumSlot() { }
    }
}
