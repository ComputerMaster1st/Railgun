using AudioChord;
using Discord;
using Finite.Commands;
using MongoDB.Bson;
using Railgun.Core;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
		[Alias("show")]
        public class MusicShow : SystemBase
        {
			private readonly MusicService _musicService;

			public MusicShow(MusicService musicService)
				=> _musicService = musicService;

			[Command]
			public async Task ExecuteAsync()
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Music;
				var songCount = 0;

				if (data.PlaylistId != ObjectId.Empty)
				{
					var playlist = await SystemUtilities.GetPlaylistAsync(_musicService, data);
					songCount = playlist.Songs.Count;
				}

				var npTc = data.NowPlayingChannel != 0 ? await Context.Guild.GetTextChannelAsync(data.NowPlayingChannel) : null;
				var autoJoinOutput = new StringBuilder();
				var badAutoJoinConfigs = new List<MusicAutoJoinConfig>();

				foreach (var autoJoinConfig in data.AutoJoinConfigs)
				{
					var vc = await Context.Guild.GetVoiceChannelAsync(autoJoinConfig.VoiceChannelId);
					var tc = await Context.Guild.GetTextChannelAsync(autoJoinConfig.TextChannelId);

					if (vc == null)
					{
						badAutoJoinConfigs.Add(autoJoinConfig);
						continue;
					}

					autoJoinOutput.AppendFormat("| {0}:{1} |", 
						vc.Name, 
						npTc == null ? (tc == null ? "[UNKNOWN TEXT CHANNEL]" : tc.Name) : "[USING \"Now Playing\" CHANNEL]");
				}

				foreach (var autoJoinConfig in badAutoJoinConfigs)
					data.AutoJoinConfigs.Remove(autoJoinConfig);

				if (data.AutoJoinConfigs.Count < 1)
				{
					autoJoinOutput.Clear();
					autoJoinOutput.AppendFormat("Not set.");
				}

				var autoDownloadOutput = data.AutoDownload ? "Enabled" : "Disabled";
				var autoSkipOutput = data.AutoSkip ? "Enabled" : "Disabled";
				var silentPlayingOutput = data.SilentNowPlaying ? "Enabled" : "Disabled";
				var silentInstallOutput = data.SilentSongProcessing ? "Enabled" : "Disabled";
				var npTcName = npTc != null ? $"#{npTc.Name}" : "None";
				var voteskipOutput = data.VoteSkipEnabled ? $"Enabled @ {data.VoteSkipLimit}% Users" : "Disabled";
				var autoPlay = string.IsNullOrWhiteSpace(data.AutoPlaySong) ? "Disabled" : data.AutoPlaySong;
				var autoLoop = data.PlaylistAutoLoop ? "Enabled" : "Disabled";
				var whitelist = data.WhitelistMode ? "Enabled" : "Disabled";
				var output = new StringBuilder();
				var roleLock = new StringBuilder();

				if (data.AllowedRoles.Count > 0)
					foreach (var allowedRole in data.AllowedRoles)
					{
						var role = Context.Guild.GetRole(allowedRole);
						roleLock.AppendFormat("| {0} |", role.Name);
					}
				else roleLock.AppendFormat("None.");

				output.AppendLine("Music Settings")
					.AppendLine()
					.AppendFormat("   Number Of Songs : {0}", songCount).AppendLine()
					.AppendLine()
					.AppendFormat("         Auto-Join : {0}", autoJoinOutput).AppendLine()
					.AppendFormat("     Auto-Download : {0}", autoDownloadOutput).AppendLine()
					.AppendFormat("         Auto-Skip : {0}", autoSkipOutput).AppendLine()
					.AppendFormat("         Auto-Play : {0}", autoPlay).AppendLine()
					.AppendFormat("Playlist Auto-Loop : {0}", autoLoop).AppendLine()
					.AppendLine()
					.AppendFormat("    Silent Running : {0}", silentPlayingOutput).AppendLine()
					.AppendFormat("    Silent Install : {0}", silentInstallOutput).AppendLine()
					.AppendLine()
					.AppendFormat("   NP Dedi Channel : {0}", npTcName).AppendLine()
					.AppendFormat("         Vote-Skip : {0}", voteskipOutput).AppendLine()
					.AppendFormat("       Role-Locked : {0}", roleLock.ToString()).AppendLine()
					.AppendFormat("      Whitelisting : {0}", whitelist);

				await ReplyAsync(Format.Code(output.ToString()));
			}
		}
    }
}
