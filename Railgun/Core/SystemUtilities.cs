using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using MongoDB.Bson;
using Railgun.Core.Enums;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Core
{
    public static class SystemUtilities
    {
        public static string GetSeparator { get; } = "⬤";
		public static string FileExtension { get; } = ".railgun";

        public static void WriteToLogFile(string directory, string message)
        {
            using (var logfile = File.AppendText($"{directory}/{DateTime.Today.ToString("yyyy-MM-dd")}.log"))
            {
                logfile.WriteLine(message);
            }
        }

        public static void LogToConsoleAndFile(LogMessage message)
        {
			ChangeConsoleColor(message.Severity);
            Console.WriteLine(message.ToString());
            WriteToLogFile(Directories.ConsoleLog, message.ToString());
        }

        public static async Task<Playlist> GetPlaylistAsync(MusicService service, ServerMusic data)
		{
			Playlist playlist;

			if (data.PlaylistId != ObjectId.Empty) {
				try {
					playlist = await service.Playlist.GetPlaylistAsync(data.PlaylistId);
					return playlist;
				} catch (ArgumentException) {
					// Failed to get playlist as it most likely doesn't exist or corrupted.
				}
			}

			playlist = new Playlist();
			data.PlaylistId = playlist.Id;

			await service.Playlist.UpdateAsync(playlist);
			return playlist;
		}

        public static async Task<Playlist> GetPlaylistAsync(MusicService service, ObjectId id) => await service.Playlist.GetPlaylistAsync(id);

		public static async Task UpdatePlaylistAsync(MusicService service, Playlist playlist) => await service.Playlist.UpdateAsync(playlist);

		public static async Task DeletePlaylistAsync(MusicService service, ObjectId playlistId) => await service.Playlist.DeleteAsync(playlistId);

        public static string GetUsernameOrMention(TreeDiagramContext db, IGuildUser user)
		{
			var profile = db.ServerProfiles.GetOrCreateData(user.GuildId);
            var sMention = profile.Globals;
			var userProfile = db.ServerProfiles.GetOrCreateData(user.Id);
            var uMention = userProfile.Globals;

			if ((sMention != null && sMention.DisableMentions) || (uMention != null && uMention.DisableMentions))
				return user.Username;
			return user.Mention;
		}

        public static async Task SendJoinLeaveMessageAsync(ServerJoinLeave data, IGuildUser user, string message, BotLog botLog)
		{
			if (string.IsNullOrEmpty(message)) return;
			if (data.SendToDM) 
            {
				try 
                {
					await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(message);
				} catch { // Ignored
				}

				return;
			}
			if (data.ChannelId == 0 && !data.SendToDM) return;

			var tc = await user.Guild.GetTextChannelAsync(data.ChannelId);

			if (tc == null) return;

			IUserMessage msg;

			try 
            {
				msg = await tc.SendMessageAsync(message);
			} 
            catch 
            {
				var output = new StringBuilder()
					.AppendFormat("<{0} <{1}>> Missing Send Message Permission!", tc.Guild.Name, tc.GuildId).AppendLine()
					.AppendFormat("---- Channel Name : {0}", tc.Name).AppendLine()
					.AppendFormat("---- Channel ID   : {0}", tc.Id).AppendLine();

				await botLog.SendBotLogAsync(BotLogType.TaskScheduler, output.ToString());
				return;
			}

			if (data.DeleteAfterMinutes > 0) {
				await Task.Run(async () => {
					await Task.Delay((int)TimeSpan.FromMinutes(data.DeleteAfterMinutes).TotalMilliseconds);

					try 
                    {
						await msg.DeleteAsync();
					} 
                    catch 
                    {
						var output2 = new StringBuilder()
							.AppendFormat("<{0} <{1}>> Missing Delete Message Permission!", tc.Guild.Name, tc.GuildId).AppendLine()
							.AppendFormat("---- Channel Name : {0}", tc.Name).AppendLine()
							.AppendFormat("---- Channel ID   : {0}", tc.Id).AppendLine();

						await botLog.SendBotLogAsync(BotLogType.TaskScheduler, output2.ToString());
					}
				});
            }
		}

		public static async Task<bool> CheckIfSelfIsHigherRoleAsync(IGuild guild, IGuildUser user)
		{
			var selfRolePosition = 0;
			var userRolePosition = 0;
			var self = await guild.GetCurrentUserAsync();

			foreach (var roleId in self.RoleIds)
			{
				var role = guild.GetRole(roleId);
				if (role.Permissions.BanMembers && role.Position > selfRolePosition)
					selfRolePosition = role.Position;
			}

			foreach (var roleId in user.RoleIds) 
			{
				var role = guild.GetRole(roleId);
				if (role.Position > userRolePosition) userRolePosition = role.Position;
			}

			if (selfRolePosition > userRolePosition) return true;
			else return false;
		}

		public static void ChangeConsoleColor(LogSeverity severity)
		{
			switch (severity) {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }
		}
    }
}