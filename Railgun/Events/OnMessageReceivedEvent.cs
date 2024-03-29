using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TreeDiagram;

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

        public void Load()
        {
            _client.MessageReceived += (message) =>
            {
                Task.Run(() => ExecuteAsync(message)).ConfigureAwait(false);
                return Task.CompletedTask;
            };
            _client.MessageUpdated += (cachedMessage, newMessage, channel) =>
            {
                Task.Run(() => ExecuteAsync(newMessage)).ConfigureAwait(false);
                return Task.CompletedTask;
            };
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

        private async Task ExecuteUpdatedAsync(SocketMessage newMsg)
        {
            _analytics.UpdatedMessages++;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var channel = newMsg.Channel as ITextChannel;
                var profile = db.ServerProfiles.GetData(channel.GuildId);

                if (profile != null && profile.Command.IgnoreModifiedMessages) return;
            }

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