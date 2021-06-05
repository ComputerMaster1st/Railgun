using System;
using System.Linq;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Attributes;
using Railgun.Music;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnVoiceStateEvent
    {
        private readonly PlayerController _players;
        private readonly MusicService _musicService;
        private readonly IServiceProvider _services;

        public OnVoiceStateEvent(DiscordShardedClient client, PlayerController players, MusicService musicService, IServiceProvider services)
        {
            _players = players;
            _musicService = musicService;
            _services = services;

            client.UserVoiceStateUpdated += (user, before, after) => Task.Factory.StartNew(async () => await ExecuteAsync(user, before, after));
        }

        private async Task ExecuteAsync(SocketUser sUser, SocketVoiceState before, SocketVoiceState after)
        {
            if (sUser.IsBot || after.VoiceChannel == null)
                return;

			var guild = after.VoiceChannel.Guild as IGuild;
			var user = await guild.GetUserAsync(sUser.Id);

            if (user.IsSelfDeafened)
                return;

            if (before.VoiceChannel != null && after.VoiceChannel != null)
                if (before.VoiceChannel.Id == after.VoiceChannel.Id)
                    return;

            if (_players.GetPlayer(guild.Id) != null || user.VoiceChannel == null)
                return;

            ServerMusic data;

			using (var scope = _services.CreateScope())
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetData(guild.Id);

                if (profile == null)
                    return;

                data = profile.Music;

                if (data.AutoJoinConfigs.Count < 1)
                    return;

                var vc = user.VoiceChannel;
                var autoJoinConfig = data.AutoJoinConfigs.FirstOrDefault(f => f.VoiceChannelId == vc.Id);

                if (autoJoinConfig == null)
                    return;

                var tc = await guild.GetTextChannelAsync(autoJoinConfig.TextChannelId);

                if (tc == null)
                    return;

                ISong song = null;

                if (!string.IsNullOrEmpty(data.AutoPlaySong))
                    song = await _musicService.GetSongAsync(SongId.Parse(data.AutoPlaySong));

                await _players.CreatePlayerAsync(user, vc, tc, true, song != null ? new SongRequest(song) : null);
			}
        }
    }
}