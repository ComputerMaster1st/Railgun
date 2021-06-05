using System;
using System.Threading.Tasks;
using AudioChord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Enums;
using Railgun.Music;
using TreeDiagram;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnGuildLeaveEvent
    {
        private readonly BotLog _botLog;
        private readonly MusicService _musicService;
        private readonly PlayerController _players;
        private readonly IServiceProvider _services;

        public OnGuildLeaveEvent(DiscordShardedClient client, BotLog botLog, MusicService musicService, PlayerController players, IServiceProvider services)
        {
            _botLog = botLog;
            _musicService = musicService;
            _players = players;
            _services = services;

            client.LeftGuild += (guild) => Task.Factory.StartNew(async () => await ExecuteAsync(guild));
        }

        private async Task ExecuteAsync(SocketGuild guild)
        {
            _players.GetPlayer(guild.Id)?.Player.CancelStream();

			using (var scope = _services.CreateScope())
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetData(guild.Id);

				if (profile != null && profile.Music.PlaylistId != ObjectId.Empty) 
                    await SystemUtilities.DeletePlaylistAsync(_musicService, profile.Music.PlaylistId);

				db.DeleteGuildData(guild.Id);
			}

			await _botLog.SendBotLogAsync(BotLogType.GuildManager, string.Format("<{0} ({1})> Left",
                guild.Name.Replace("@", "(at)"),
                guild.Id));
        }
    }
}