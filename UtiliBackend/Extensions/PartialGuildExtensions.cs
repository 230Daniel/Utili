using Disqord;

namespace UtiliBackend.Extensions
{
    public static class PartialGuildExtensions
    {
        public static string GetIconUrl(this IPartialGuild source)
        {
            return string.IsNullOrWhiteSpace(source.IconHash)
                ? "https://cdn.discordapp.com/embed/avatars/1.png"
                : $"https://cdn.discordapp.com/icons/{source.Id}/{source.IconHash}.png?size=256";
        }
    }
}
