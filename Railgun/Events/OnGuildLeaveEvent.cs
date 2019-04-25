using System;
using System.Threading.Tasks;
using AudioChord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Railgun.Core.Enums;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Events
{
    public class OnGuildLeaveEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;
        private readonly MusicService _musicService;
        private readonly PlayerController _playerController;

        public OnGuildLeaveEvent(DiscordShardedClient client, BotLog botLog, IServiceProvider services)
        {
            _client = client;
            _botLog = botLog;
            _services = services;

            _musicService = services.GetService<MusicService>();
            _playerController = services.GetService<PlayerController>();
        }

        public void Load() => _client.LeftGuild += (guild) => Task.Factory.StartNew(() => ExecuteAsync(guild));

        private Task ExecuteAsync(SocketGuild guild)
        {
            var container = _playerController.GetPlayer(guild.Id);
            if (container == null) return Task.CompletedTask;

            container.Player.CancelStream();

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var data = db.ServerMusics.GetData(guild.Id);

				if (data != null && data.PlaylistId != ObjectId.Empty) _musicService.Playlist.DeleteAsync(data.PlaylistId).GetAwaiter();

				db.DeleteGuildData(guild.Id);
			}

			_botLog.SendBotLogAsync(BotLogType.GuildManager, $"<{guild.Name} ({guild.Id})> Left").GetAwaiter();
            return Task.CompletedTask;
        }
    }
}