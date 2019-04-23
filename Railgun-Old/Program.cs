using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Configuration;

namespace Railgun {

    class Program 
    {
        public static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private async Task StartAsync() {
            var masterConfig = await MasterConfig.LoadAsync();
            var client = new DiscordShardedClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            var initializer = new Initializer(masterConfig, client);

            await initializer.InitializeAsync();
            await client.LoginAsync(TokenType.Bot, masterConfig.DiscordConfig.Token);
            await client.StartAsync();
            await client.SetStatusAsync(UserStatus.Idle);
            await client.SetGameAsync("Booting System...");

            await Task.Delay(-1);
        }
    }
}