using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using TreeDiagram;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.User;

namespace Railgun.Core.Utilities
{
	public class CommandUtils
	{
		private readonly IServiceProvider _services;
		private readonly MusicService _musicService;

		public CommandUtils(IServiceProvider services)
		{
			_services = services;

			_musicService = _services.GetService<MusicService>();
		}

		public string GetUsernameOrMention(IGuildUser user)
		{
			ServerMention sMention;
			UserMention uMention;

			using (var scope = _services.CreateScope()) {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

				sMention = db.ServerMentions.GetData(user.GuildId);
				uMention = db.UserMentions.GetData(user.Id);
			}

			if ((sMention != null && sMention.DisableMentions) || (uMention != null && uMention.DisableMentions))
				return user.Username;
			return user.Mention;
		}

		public async Task<Playlist> GetPlaylistAsync(ServerMusic data)
		{
			if (data.PlaylistId != ObjectId.Empty)
				return await _musicService.Playlist.GetPlaylistAsync(data.PlaylistId);

			var playlist = new Playlist();

			data.PlaylistId = playlist.Id;

			await _musicService.Playlist.UpdateAsync(playlist);

			return playlist;
		}

		public async Task<bool> CheckIfSelfIsHigherRole(IGuild guild, IGuildUser user)
		{
			var selfRolePosition = 0;
			var userRolePosition = 0;
			var self = await guild.GetCurrentUserAsync();

			foreach (var roleId in self.RoleIds) {
				var role = guild.GetRole(roleId);

				if (role.Permissions.BanMembers && role.Position > selfRolePosition)
					selfRolePosition = role.Position;
			}

			foreach (var roleId in user.RoleIds) {
				var role = guild.GetRole(roleId);

				if (role.Position > userRolePosition) userRolePosition = role.Position;
			}

			if (selfRolePosition > userRolePosition) return true;
			else return false;
		}

		public static async Task SendStringAsFileAsync(ITextChannel tc, string filename, string output, string msgText = null, bool includeGuildName = true)
		{
			var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(output));
			var outputFilename = (includeGuildName ? $"{tc.Guild.Name}-" : "") + filename;

			await tc.SendFileAsync(outputStream, outputFilename, msgText);
		}
	}
}