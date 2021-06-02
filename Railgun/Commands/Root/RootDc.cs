using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("dc")]
        public class RootDc : SystemBase
        {
            private readonly PlayerController _players;

            [Command]
            public async Task ExecuteAsync([Remainder] string msg)
            {
                var client = Context.Client as DiscordShardedClient;

                await client.SetStatusAsync(UserStatus.DoNotDisturb);
                await client.SetGameAsync("Shutting Down ...");
                await ReplyAsync("Disconnecting ...");

                if (_players.PlayerContainers.Count > 0)
                {
                    try
                    {
                        var output = new StringBuilder()
                            .AppendFormat("{0} : Stopping music stream due to the following reason... {1}", Format.Bold("WARNING"), string.IsNullOrWhiteSpace(msg) ? Format.Bold("System Restart") : Format.Bold(msg));

                        foreach (var playerInfo in _players.PlayerContainers)
                        {
                            await playerInfo.TextChannel.SendMessageAsync(output.ToString());
                            playerInfo.Player.CancelStream();
                        }
                    }
                    catch { }
                }

                while (_players.PlayerContainers.Count > 0)
                    await Task.Delay(1000);

                await client.StopAsync();
                await client.LogoutAsync();
            }

            [Command]
            public Task ExecuteAsync()
                => ExecuteAsync(null);
        }
    }
}
