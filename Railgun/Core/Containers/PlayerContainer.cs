using System.Threading;
using Discord;
using Railgun.Core.Music;

namespace Railgun.Core.Containers
{
    public class PlayerContainer
    {
        public ulong GuildId { get; }
        public ITextChannel TextChannel { get; }
        public Player Player { get; }
        public IUserMessage LogEntry { get; set; }
        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1);

        public PlayerContainer(ITextChannel tc, Player player) {
            GuildId = tc.GuildId;
            TextChannel = tc;
            Player = player;
        }
    }
}