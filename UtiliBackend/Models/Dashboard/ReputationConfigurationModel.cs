using System.Collections.Generic;
using System.Linq;
using Database.Entities;

namespace UtiliBackend.Models
{
    public class ReputationConfigurationModel
    {
        public List<ReputationConfigurationEmojiModel> Emojis { get; set; }

        public void ApplyTo(ReputationConfiguration configuration)
        {
            foreach (var emojiModel in Emojis)
            {
                var emojiConfiguration = configuration.Emojis.FirstOrDefault(x => x.Emoji == emojiModel.Emoji);
                if (emojiConfiguration is null)
                {
                    emojiConfiguration = new ReputationConfigurationEmoji(emojiModel.Emoji)
                    {
                        Value = emojiModel.Value
                    };
                    configuration.Emojis.Add(emojiConfiguration);
                }
                else
                {
                    emojiConfiguration.Value = emojiModel.Value;
                }
            }

            configuration.Emojis.RemoveAll(x => Emojis.All(y => x.Emoji != y.Emoji));
        }
    }
    
    public class ReputationConfigurationEmojiModel
    {
        public string Emoji { get; set; }
        public int Value { get; set; }
    }
}
