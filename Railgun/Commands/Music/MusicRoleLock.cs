using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
	{
		[Alias("rolelock"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicRoleLock : SystemBase
		{
			[Command("add")]
			public Task AddRoleAsync(IRole role)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
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
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
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