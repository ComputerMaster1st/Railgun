using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Configuration;

namespace Railgun
{
    class Program
    {
        private MasterConfig _config = null;
        private readonly DiscordShardedClient _client = new DiscordShardedClient(
            new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            }
        );

        private Kernel _kernel = null;

        static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        private async Task StartAsync()
        {
            Directories.CheckDirectories();
            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Booting System..."));

            _config = MasterConfig.Load();
            _kernel = new Kernel(_config, _client);
            _kernel.Boot();

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "System Booted!"));

            await _client.LoginAsync(TokenType.Bot, _config.DiscordConfig.Token);
            await _client.StartAsync();
            await _client.SetStatusAsync(UserStatus.Idle);
            await _client.SetGameAsync("Booting System...");

            await Task.Delay(-1);
        }
    }
}
