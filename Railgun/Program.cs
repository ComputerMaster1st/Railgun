﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Configuration;

namespace Railgun {

    class Program 
    {
        public static void main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        private async Task StartAsync() {
            Console.WriteLine("Starting TreeDiagram Operating System...");

            var masterConfig = await MasterConfig.LoadAsync();
            var client = new DiscordShardedClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });
            var initializer = new Initializer(masterConfig, client);

            await initializer.InitializeCommandsAsync();
            await client.LoginAsync(TokenType.Bot, masterConfig.DiscordConfig.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }
    }
}