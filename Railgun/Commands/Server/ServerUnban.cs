using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
        [Alias("unban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)]
        public class ServerUnban : SystemBase
        {
            private readonly BotLog _botLog;

            public ServerUnban(BotLog botLog)
                => _botLog = botLog;

            [Command]
            public async Task ExecuteAsync(ulong user)
            {
                await Context.Guild.RemoveBanAsync(user);

                await ReplyAsync($"User ID {Format.Bold(user.ToString())} is now unbanned from the server.");

                await _botLog.SendBotLogAsync(BotLogType.Common, string.Format("User Unbanned {0} <{1} ({2})> ID : {3}",
                    SystemUtilities.GetSeparator,
                    Context.Guild.Name,
                    Context.Guild.Id,
                    user));
            }
        }
    }
}
