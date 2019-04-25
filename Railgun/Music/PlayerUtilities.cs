using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;

namespace Railgun.Music
{
    public static class PlayerUtilities
    {
        public static async Task CreateOrModifyMusicPlayerLogEntryAsync(MasterConfig config, IDiscordClient client, PlayerContainer container) {
			var song = container.Player.CurrentSong;
			var songId = "N/A";
			var songName = "N/A";
			var songLength = "N/A";
			var songStarted = "N/A";

			if (song != null) {
				songId = song.Id.ToString();
				songName = song.Metadata.Name;
				songLength = song.Metadata.Length.ToString();
				songStarted = container.Player.SongStartedAt.ToString(CultureInfo.CurrentCulture);
			}
			
			var output = new StringBuilder()
				.AppendFormat("Music Player {0} <{1} <{2}>>", SystemUtilities.GetSeparator, container.TextChannel.Guild.Name,
					container.TextChannel.GuildId).AppendLine()
				.AppendFormat("---- Created At      : {0}", container.Player.CreatedAt).AppendLine()
				.AppendFormat("---- Latency         : {0}ms", container.Player.Latency).AppendLine()
				.AppendFormat("---- Status          : {0}", container.Player.Status).AppendLine()
				.AppendFormat("---- Song Started At : {0}", songStarted).AppendLine()
				.AppendFormat("---- Song ID         : {0}", songId).AppendLine()
				.AppendFormat("---- Song Name       : {0}", songName).AppendLine()
				.AppendFormat("---- Song Length     : {0}", songLength).AppendLine();

			var formattedOutput = Format.Code(output.ToString());

			try
			{
				if (container.LogEntry != null)
				{
					await container.LogEntry.ModifyAsync((x) => x.Content = formattedOutput);
					return;
				}

				if (config.DiscordConfig.BotLogChannels.MusicPlayerActive == 0) return;

				var masterGuild = await client.GetGuildAsync(config.DiscordConfig.MasterGuildId);
				if (masterGuild == null) return;
				var logTc = await masterGuild.GetTextChannelAsync(config.DiscordConfig.BotLogChannels.MusicPlayerActive);
				if (logTc == null) return;

				container.LogEntry = await logTc.SendMessageAsync(formattedOutput);
			}
			catch
			{
				// Ignore
			}
		}
    }
}