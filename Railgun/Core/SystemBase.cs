using System.Threading.Tasks;
using Discord;
using Finite.Commands;

namespace Railgun.Core
{
    public class SystemBase : ModuleBase<SystemContext>
    { 
        public async Task<IUserMessage> ReplyAsync(string msg = null, bool isTTS = false, Embed embed = null) 
            => await Context.Channel.SendMessageAsync(msg, isTTS, embed);
    }
}