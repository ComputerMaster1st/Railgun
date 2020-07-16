using System;
using System.Threading.Tasks;
using AudioChord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Railgun.Core;
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

        public void Load() => _client.LeftGuild += (guild) => Task.Factory.StartNew(async () => await ExecuteAsync(guild));

        private async Task ExecuteAsync(SocketGuild guild)
        {
            _playerController.GetPlayer(guild.Id)?.Player.CancelStream();

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetOrCreateData(guild.Id);
                var data = profile.Music;

				if (data != null && data.PlaylistId != ObjectId.Empty) await SystemUtilities.DeletePlaylistAsync(_musicService, data.PlaylistId);

				db.DeleteGuildData(guild.Id);
			}

			await _botLog.SendBotLogAsync(BotLogType.GuildManager, $"<{guild.Name.Replace("@", "(at)")} ({guild.Id})> Left");
        }
    }
}