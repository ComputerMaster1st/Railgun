using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("rolelock"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicRoleLock : SystemBase
		{
			private ServerMusic GetData(ulong guildId, bool create = false)
			{
				ServerProfile data;

				if (create)
					data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
				else {
					data = Context.Database.ServerProfiles.GetData(guildId);

					if (data == null) 
						return null;
				}

				if (data.Music == null)
					if (create)
						data.Music = new ServerMusic();
				
				return data.Music;
			}

			[Command("add")]
			public Task AddRoleAsync(IRole role)
			{
				var data = GetData(Context.Guild.Id, true);
				data.AllowedRoles.Add(role.Id);

				if (data.AllowedRoles.Count < 2) 
					return ReplyAsync($"All music commands are now role-locked to {role.Name}.");
				return ReplyAsync($"Users with the role {Format.Bold(role.Name)}, may also use the music commands.");
			}
			
			[Command("add")]
			public Task AddRoleAsync([Remainder] string roleName)
			{
				foreach (var role in Context.Guild.Roles)
					if (role.Name.Contains(roleName))
						return AddRoleAsync(role);
				return ReplyAsync("Unable to find a role with the name you specified.");
			}

			[Command("remove")]
			public Task RemoveRoleAsync(IRole role)
			{
				var data = GetData(Context.Guild.Id, true);
				var count = data.AllowedRoles.RemoveAll(allowedRole => allowedRole == role.Id);

				if (count < 1)
					return ReplyAsync("The role specified was never role-locked.");

				var output = new StringBuilder()
					.AppendFormat("The role {0}, has now been removed from role-locking.", role.Name).AppendLine();

				if (data.AllowedRoles.Count < 1) output.AppendLine("All music commands are no longer role-locked.");

				return ReplyAsync(output.ToString());
			}
			
			[Command("remove")]
			public Task RemoveRoleAsync([Remainder] string roleName)
			{
				foreach (var role in Context.Guild.Roles)
					if (role.Name.Contains(roleName))
						return RemoveRoleAsync(role);
				return ReplyAsync("Unable to find a role with the name you specified.");
			}
		}
	}
}