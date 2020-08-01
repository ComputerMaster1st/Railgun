using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Music.PlayerEventArgs;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Music.Events
{
    public class PlayingEvent : IPlayerEvent
    {
        private readonly MasterConfig _config;
        private readonly IDiscordClient _client;
        private readonly IServiceProvider _services;
        private PlayerContainer _container;

        public PlayingEvent(MasterConfig config, IDiscordClient client, IServiceProvider services)
        {
            _config = config;
            _client = client;
            _services = services;
        }

        public void Load(PlayerContainer container)
		{
            _container = container;
			_container.Player.Playing += async (s, a) => await ExecuteAsync(a);
		}

        private async Task ExecuteAsync(PlayingEventArgs args)
        {
			try {
				ServerMusic data;
				ITextChannel tc;
				
				using (var scope = _services.CreateScope()) {
					var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
					var profile = db.ServerProfiles.GetData(args.GuildId);
                    data = profile.Music;

					if (data.NowPlayingChannel != 0)
						tc = await (await _client.GetGuildAsync(args.GuildId)).GetTextChannelAsync(data.NowPlayingChannel);
					else tc = _container.TextChannel;

					if (!data.SilentNowPlaying) {
						var output = new StringBuilder()
							.AppendFormat("Now Playing: {0} {1} ID: {2}", Format.Bold(args.Song.Metadata.Title), SystemUtilities.GetSeparator, Format.Bold(args.Song.Metadata.Id.ToString())).AppendLine()
							.AppendFormat("Time: {0} {1} Uploader: {2} {1} URL: {3}", Format.Bold(args.Song.Metadata.Duration.ToString()), SystemUtilities.GetSeparator, Format.Bold(args.Song.Metadata.Uploader), Format.Bold($"<{args.Song.Metadata.Source}>"));

						if (args.IsRatelimited) output.AppendLine().AppendLine(Format.Italics(Format.Bold("WARNING :") + " YouTube Rate-Limits (429) are currently in effect. Some songs may not work, however will still remain inside the playlist and be encoded/downloaded once the rate-limit has been cleared."));

						await tc.SendMessageAsync(output.ToString());
					}
				}

				await PlayerUtilities.CreateOrModifyMusicPlayerLogEntryAsync(_config, _client, _container);
			} catch {
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				_container.Player.CancelStream();
			}
        }
    }
}