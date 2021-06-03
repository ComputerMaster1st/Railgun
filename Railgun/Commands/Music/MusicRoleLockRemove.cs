using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicRoleLock
        {
			[Alias("remove")]
            public class MusicRoleLockRemove : SystemBase
            {
				[Command]
				public Task ExecuteAsync(IRole role)
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;
					var removed = data.RemoveAllowedRole(role.Id);

					if (!removed)
						return ReplyAsync("The role specified was never role-locked.");

					var output = new StringBuilder()
						.AppendFormat("The role {0}, has now been removed from role-locking.", role.Name).AppendLine();

					if (data.AllowedRoles.Count < 1) 
						output.AppendLine("All music commands are no longer role-locked.");

					return ReplyAsync(output.ToString());
				}

				[Command("remove")]
				public Task ExecuteAsync([Remainder] string roleName)
				{
					foreach (var role in Context.Guild.Roles)
						if (role.Name.Contains(roleName))
							return ExecuteAsync(role);

					return ReplyAsync("Unable to find a role with the name you specified.");
				}
			}
        }
    }
}
