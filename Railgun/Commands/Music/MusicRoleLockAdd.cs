using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicRoleLock
        {
			[Alias("add")]
            public class MusicRoleLockAdd : SystemBase
            {
				[Command]
				public Task ExecuteAsync(IRole role)
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;

					data.AddAllowedRole(role.Id);

					if (data.AllowedRoles.Count < 2)
						return ReplyAsync($"All music commands are now role-locked to {role.Name}.");

					return ReplyAsync($"Users with the role {Format.Bold(role.Name)}, may also use the music commands.");
				}

				[Command]
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
