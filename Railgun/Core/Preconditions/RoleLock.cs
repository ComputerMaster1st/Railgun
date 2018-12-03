using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Enums;
using TreeDiagram;

namespace Railgun.Core.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleLock : PreconditionAttribute
    {
        private readonly ModuleType _moduleType;

        public RoleLock(ModuleType modType) => _moduleType = modType;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            var user = (IGuildUser)context.User;
            var output = new StringBuilder();

            using (var db = services.GetService<TreeDiagramContext>()) {
                switch (_moduleType) {
                    case ModuleType.Music:
                        var data = await db.ServerMusics.GetAsync(context.Guild.Id);

                        if (data == null || data.AllowedRoles.Count < 1) 
                            return await Task.FromResult(PreconditionResult.FromSuccess());

                        var tempOutput = new StringBuilder();

                        foreach (var allowedRole in data.AllowedRoles) {
                            var role = context.Guild.GetRole(allowedRole.RoleId);

                            tempOutput.AppendLine($"| {role.Name} |");

                            if (user.RoleIds.Contains(allowedRole.RoleId))
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        output.AppendLine("This command is locked to specific role(s). You must have the following role(s)...")
                            .AppendLine()
                            .AppendLine(tempOutput.ToString());
                        break;
                }
            }

            return await Task.FromResult(PreconditionResult.FromError(output.ToString()));
        }
    }
}