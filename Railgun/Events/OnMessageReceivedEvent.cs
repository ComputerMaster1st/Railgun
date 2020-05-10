using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Utilities;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Events
{
    public class OnMessageReceivedEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly Analytics _analytics;
        private readonly IServiceProvider _services;
        private readonly List<IOnMessageSubEvent> _subEvents = new List<IOnMessageSubEvent>();

        public OnMessageReceivedEvent(DiscordShardedClient client, Analytics analytics, IServiceProvider services)
        {
            _client = client;
            _analytics = analytics;
            _services = services;
        }

        public void Load() {
            _client.MessageReceived += (message) => Task.Factory.StartNew(async () => await ExecuteReceivedAsync(message));
            _client.MessageUpdated += (cachedMessage, newMessage, channel) => Task.Factory.StartNew(async () => await ExecuteUpdatedAsync(cachedMessage, newMessage));
        }

        public OnMessageReceivedEvent AddSubEvent(IOnMessageSubEvent sEvent)
        {
            _subEvents.Add(sEvent);
            return this;
        }

        private async Task ExecuteReceivedAsync(SocketMessage message)
        {
            _analytics.ReceivedMessages++;
            await ExecuteAsync(message);
        }

        private async Task ExecuteUpdatedAsync(Cacheable<IMessage, ulong> cachedMsg, SocketMessage newMsg)
        {
            _analytics.UpdatedMessages++;

            var oldMsg = await cachedMsg.GetOrDownloadAsync();
            ServerCommand data;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var channel = newMsg.Channel as ITextChannel;
                data = db.ServerCommands.GetData(channel.GuildId);
            }

            if (data.IgnoreModifiedMessages) return;
            await ExecuteAsync(newMsg);          
        }

        private async Task ExecuteAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage) || !(message.Channel is SocketGuildChannel) || string.IsNullOrEmpty(message.Content)) return;

            foreach (var subEvent in _subEvents)
                await subEvent.ExecuteAsync(message);
        }
    }
}