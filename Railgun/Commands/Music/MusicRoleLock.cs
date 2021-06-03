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
		public partial class MusicRoleLock : SystemBase
		{
			[Command("add")]
			public Task AddRoleAsync(IRole role)
			{
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            	var data = profile.Music;
				data.AddAllowedRole(role.Id);

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
		}
	}
}