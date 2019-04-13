using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Extensions;

namespace Railgun.Core.Commands
{
    public class SystemBase : ModuleBase<SystemContext> { 
        public async Task<IUserMessage> ReplyAsync(string msg = null, bool isTTS = false, Embed embed = null) 
            => await (Context.Channel as ITextChannel).TrySendMessageAsync(msg, isTTS, embed);
    }
}