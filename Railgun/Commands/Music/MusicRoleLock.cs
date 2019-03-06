using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Music
{
	public partial class Music
	{
		[Alias("rolelock"), UserPerms(GuildPermission.ManageGuild)]
		public class MusicRoleLock : SystemBase
		{
			[Command("add")]
			public async Task AddRoleAsync(IRole role)
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);

				data.AllowedRoles.Add(new UlongRoleId(role.Id));

				if (data.AllowedRoles.Count < 2) {
					await ReplyAsync($"All music commands are now role-locked to {role.Name}.");

					return;
				}

				await ReplyAsync($"Users with the role {Format.Bold(role.Name)}, may also use the music commands.");
			}
			
			[Command("add")]
			public async Task AddRoleAsync([Remainder] string roleName)
			{
				foreach (var role in Context.Guild.Roles)
					if (role.Name.Contains(roleName)) {
						await AddRoleAsync(role);

						return;
					}

				await ReplyAsync("Unable to find a role with the name you specified.");
			}

			[Command("remove")]
			public async Task RemoveRoleAsync(IRole role)
			{
				var data = Context.Database.ServerMusics.GetOrCreateData(Context.Guild.Id);
				var count = data.AllowedRoles.RemoveAll(allowedRole => allowedRole.RoleId == role.Id);

				if (count < 1) {
					await ReplyAsync("The role specified was never role-locked.");

					return;
				}

				var output = new StringBuilder()
					.AppendFormat("The role {0}, has now been removed from role-locking.", role.Name).AppendLine();

				if (data.AllowedRoles.Count < 1) output.AppendLine("All music commands are no longer role-locked.");

				await ReplyAsync(output.ToString());
			}
			
			[Command("remove")]
			public async Task RemoveRoleAsync([Remainder] string roleName)
			{
				foreach (var role in Context.Guild.Roles)
					if (role.Name.Contains(roleName)) {
						await RemoveRoleAsync(role);

						return;
					}

				await ReplyAsync("Unable to find a role with the name you specified.");
			}
		}
	}
}