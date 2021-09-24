using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    [Alias("root"), BotAdmin]
    public partial class Root : SystemBase
    {
        private readonly DiscordShardedClient _client;

        public Root(DiscordShardedClient client)
        {
            _client = client;
        }

        [Command]
        public Task ExecuteAsync()
            => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");

        [Command("selfdestruct")]
        public async Task DestructAsync()
        {
            var output = new StringBuilder()
                .AppendLine(Format.Bold("WARNING! THIS BOT ACCOUNT HAS BEEN COMPROMISED BY THE SAME HIJACKER WHO HIJACKED THE ACCOUNT 'Nekoputer#0001' (UserID: 166264102526779392)."))
                .AppendLine("The real owner is waiting from Discord's Trust & Safety to gain back control of the account.")
                .AppendLine(Format.Bold("DO NOT CONTACT 'Nekoputer#0001' (UserID: 166264102526779392) UNTIL I HAVE PROVEN TO HAVE MY ACCOUNT BACK!"))
                .AppendLine("Thank you for supporting Railgun and sorry for all the inconveniences caused by this worthless 'script kiddie' who took my account.");

            foreach (var guild in _client.Guilds)
            {
                var self = await ((IGuild)guild).GetUserAsync(_client.CurrentUser.Id);

                foreach (var chn in guild.Channels)
                {
                    if (chn is ITextChannel)
                    {
                        ITextChannel channel = chn as ITextChannel;
                        var perms = self.GetPermissions(channel);

                        if (perms.SendMessages)
                        {
                            await channel.SendMessageAsync(output.ToString());
                            break;
                        }
                    }
                }

                await Task.Delay(1000);
                await guild.LeaveAsync();
            }
        }
    }
}