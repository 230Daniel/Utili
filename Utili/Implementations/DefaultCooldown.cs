using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
