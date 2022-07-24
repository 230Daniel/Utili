using Disqord.Bot.Commands;
using Qmmands;

namespace Utili.Bot.Commands;

internal class DefaultRateLimit : RateLimitAttribute
{
    public DefaultRateLimit(int amount, int per)
        : base(amount, per, RateLimitMeasure.Seconds, RateLimitBucketType.Channel)
    {
    }
}
