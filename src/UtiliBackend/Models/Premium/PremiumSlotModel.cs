using Database.Entities;

namespace UtiliBackend.Models
{
    public class PremiumSlotModel
    {
        public int SlotId { get; set; }
        public string GuildId { get; set; }

        public void ApplyTo(PremiumSlot slot)
        {
            slot.GuildId = ulong.Parse(GuildId);
        }
    }
}
