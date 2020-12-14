using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Results;
using Railgun.Core.Enums;
using TreeDiagram;
using Railgun.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Railgun.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RoleLock : Attribute, IPreconditionAttribute
	{
		private readonly ModuleType _moduleType;

		public RoleLock(ModuleType modType) => _moduleType = modType;

		public Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services)
		{
			var user = context.Author as IGuildUser;
			var output = new StringBuilder();

			// Bot Admin Override
			var config = services.GetService<MasterConfig>();
			if (config.DiscordConfig.MasterAdminId == user.Id || config.DiscordConfig.OtherAdmins.Contains(user.Id))
				return Task.FromResult(PreconditionResult.FromSuccess());

			// Server Owner Override
			if (user.Guild.OwnerId == user.Id)
				return Task.FromResult(PreconditionResult.FromSuccess());

			switch (_moduleType) 
			{
				case ModuleType.Music:
					var profile = context.Database.ServerProfiles.GetData(context.Guild.Id);
					if (profile == null) return Task.FromResult(PreconditionResult.FromSuccess());
					
            		var data = profile.Music;

					if (data.AllowedRoles.Count < 1) return Task.FromResult(PreconditionResult.FromSuccess());

					var tempOutput = new StringBuilder();

					foreach (var allowedRole in data.AllowedRoles) {
						var role = context.Guild.GetRole(allowedRole);

						tempOutput.AppendLine($"| {role.Name} |");

						if (user.RoleIds.Contains(allowedRole)) return Task.FromResult(PreconditionResult.FromSuccess());
					}

					output.AppendLine("This command is locked to specific role(s). You must have the following role(s)...")
						.AppendLine()
						.AppendLine(tempOutput.ToString());
					break;
			}

			return Task.FromResult(PreconditionResult.FromError(output.ToString()));
		}
	}
}