using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Commands.Results;
using Railgun.Core.Enums;
using TreeDiagram;

namespace Railgun.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleLock : Attribute, IPreconditionAttribute
    {
        private readonly ModuleType _moduleType;

        public RoleLock(ModuleType modType) => _moduleType = modType;

        public async Task<PreconditionResult> CheckPermissionsAsync(SystemContext context, CommandInfo command, IServiceProvider services) {
            var user = (IGuildUser)context.Author;
            var output = new StringBuilder();

            using (var db = services.GetService<TreeDiagramContext>()) {
                switch (_moduleType) {
                    case ModuleType.Music:
                        var data = await db.ServerMusics.GetAsync(context.Guild.Id);

                        if (data == null || data.AllowedRoles.Count < 1) return PreconditionResult.FromSuccess();

                        var tempOutput = new StringBuilder();

                        foreach (var allowedRole in data.AllowedRoles) {
                            var role = context.Guild.GetRole(allowedRole.RoleId);

                            tempOutput.AppendLine($"| {role.Name} |");

                            if (user.RoleIds.Contains(allowedRole.RoleId)) return PreconditionResult.FromSuccess();
                        }

                        output.AppendLine("This command is locked to specific role(s). You must have the following role(s)...")
                            .AppendLine()
                            .AppendLine(tempOutput.ToString());
                        break;
                }
            }

            return PreconditionResult.FromError(output.ToString());
        }
    }
}