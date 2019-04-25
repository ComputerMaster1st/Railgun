using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Music;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Events
{
    public class OnVoiceStateEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly PlayerController _controller;
        private readonly IServiceProvider _services;

        public OnVoiceStateEvent(DiscordShardedClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            _controller = services.GetService<PlayerController>();
        }

        public void Load() => _client.UserVoiceStateUpdated += (user, before, after) => Task.Factory.StartNew(async () => await ExecuteAsync(user, after));

        private async Task ExecuteAsync(SocketUser sUser, SocketVoiceState after)
        {
            if (sUser.IsBot || after.VoiceChannel == null) return;

			var guild = after.VoiceChannel.Guild as IGuild;
			var user = await guild.GetUserAsync(sUser.Id);

			if (_controller.GetPlayer(guild.Id) != null || user.VoiceChannel == null) return;

			ServerMusic data;

			using (var scope = _services.CreateScope())
            {
				data = scope.ServiceProvider.GetService<TreeDiagramContext>().ServerMusics.GetData(guild.Id);
			}

			if (data == null) return;

			var tc = data.AutoTextChannel != 0 ? await guild.GetTextChannelAsync(data.AutoTextChannel) : null;
			var vc = user.VoiceChannel;

			if (vc.Id == data.AutoVoiceChannel && tc != null) await _controller.CreatePlayerAsync(user, vc, tc, true);
        }
    }
}