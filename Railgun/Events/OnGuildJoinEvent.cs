using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Core.Enums;

namespace Railgun.Events
{
    public class OnGuildJoinEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BotLog _botLog;

        public OnGuildJoinEvent(DiscordShardedClient client, BotLog botLog)
        {
            _client = client;
            _botLog = botLog;
        }

        public void Load() => _client.JoinedGuild += (guild) => Task.Factory.StartNew(async () => await ExecuteAsync(guild));

        private Task ExecuteAsync(SocketGuild guild)
            => _botLog.SendBotLogAsync(BotLogType.GuildManager, $"<{guild.Name.Replace("@", "(at)")} ({guild.Id})> Joined");
    }
}