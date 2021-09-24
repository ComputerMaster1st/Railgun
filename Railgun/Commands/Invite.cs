using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands
{
    [Alias("invite")]
    public class Invite : SystemBase
    {
        private readonly DiscordShardedClient _client;

        public Invite(DiscordShardedClient client)
            => _client = client;

        [Command]
        public Task ExecuteAsync()
        {
            var output = new StringBuilder()
                .AppendFormat("Railgun Invite Link: https://discordapp.com/api/oauth2/authorize?client_id={0}&scope=bot", _client.CurrentUser.Id).AppendLine()
                .AppendLine(Format.Bold("PLEASE NOTE: Railgun#2440 (261878358625746964) has been hijacked along with Nekoputer#0001 (166264102526779392). Please visit our new Discord server for any new updates!"));

            return ReplyAsync(output.ToString());
        }
    }
}