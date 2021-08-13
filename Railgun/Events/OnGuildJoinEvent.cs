using Discord.WebSocket;
using Railgun.Core.Enums;
using System.Threading.Tasks;

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

        public void Load() => _client.JoinedGuild += (guild) =>
        {
            Task.Run(() => ExecuteAsync(guild)).ConfigureAwait(false);
            return Task.CompletedTask;
        };

        private Task ExecuteAsync(SocketGuild guild)
            => _botLog.SendBotLogAsync(BotLogType.GuildManager, $"<{guild.Name.Replace("@", "(at)")} ({guild.Id})> Joined");
    }
}