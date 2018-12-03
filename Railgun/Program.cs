using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core.Configuration;

namespace Railgun {

    class Program 
    {
        public static void main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private async Task StartAsync() {
            Console.WriteLine("Starting TreeDiagram Operating System...");

            var masterConfig = MasterConfig.LoadAsync();
            var client = new DiscordShardedClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            

        //     _sysManager = New Initializer(_config, _client)
        // Await _sysManager.InitializeCommandsAsync()
        
        // Await _client.LoginAsync(TokenType.Bot, _config.DiscordConfig.Token)
        // Await _client.StartAsync()
        // Await Task.Delay(-1)
        }
    }
}