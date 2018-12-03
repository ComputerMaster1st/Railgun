using Discord.Commands;

namespace Railgun.Core.Commands
{
    public class SystemBase : ModuleBase<SystemContext>
    {
        protected override void AfterExecute(CommandInfo command) {
            Context.DisposeDatabase();
            base.AfterExecute(command);
        }
    }
}