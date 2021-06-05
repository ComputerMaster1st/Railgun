using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnGuildJoinEvent
    {
        private readonly BotLog _botLog;

        public OnGuildJoinEvent(DiscordShardedClient client, BotLog botLog)
        {
            _botLog = botLog;

            client.JoinedGuild += (guild) => Task.Factory.StartNew(async () => await ExecuteAsync(guild));
        }

        private Task ExecuteAsync(SocketGuild guild)
            => _botLog.SendBotLogAsync(BotLogType.GuildManager, string.Format("<{0} ({1})> Joined",
                guild.Name.Replace("@", "(at)"),
                guild.Id));
    }
}