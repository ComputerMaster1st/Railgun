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
using TreeDiagram.Models.User;

namespace Railgun.Core
{
    public static class SystemUtilities
    {
        public static string GetSeparator { get; } = "⬤";

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

        public static Task<Playlist> GetPlaylistAsync(MusicService service, ServerMusic data)
		{
			if (data.PlaylistId != ObjectId.Empty) return service.Playlist.GetPlaylistAsync(data.PlaylistId);

			var playlist = new Playlist();

			data.PlaylistId = playlist.Id;

			service.Playlist.UpdateAsync(playlist).GetAwaiter().GetResult();

			return Task.FromResult(playlist);
		}

        public static string GetUsernameOrMention(TreeDiagramContext db, IGuildUser user)
		{
			ServerMention sMention = db.ServerMentions.GetData(user.GuildId);;
			UserMention uMention = db.UserMentions.GetData(user.Id);

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

		public static async Task<bool> CheckIfSelfIsHigherRole(IGuild guild, IGuildUser user)
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