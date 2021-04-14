using Disqord.Bot;
using Qmmands;

namespace Utili.Implementations
{
    class DefaultCooldown : CooldownAttribute
    {
        public DefaultCooldown(int amount, int per)
            : base(amount, per, CooldownMeasure.Seconds, CooldownBucketType.Channel)
        { }
    }
}
