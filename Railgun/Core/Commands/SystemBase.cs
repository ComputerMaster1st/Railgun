using System.Threading.Tasks;
using Discord;
using Finite.Commands;

namespace Railgun.Core.Commands
{
    public class SystemBase : ModuleBase<SystemContext> { 
        public async Task<IUserMessage> ReplyAsync(string msg) 
            => await Context.Channel.SendMessageAsync(msg);
    }
}