using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("donate")]
    public class Dontate : SystemBase
    {
        [Command]
        public Task ExecuteAsync() {
            var output = new StringBuilder()
                .AppendLine("Railgun is hosted on a dedicated server which is paid monthly and we never ask for a dime. However, if you still wish to give something to us, feel free to use the following URL.")
                .AppendFormat("Tip Link: {0}", Format.EscapeUrl("https://streamlabs.com/computermaster1st/tip")).AppendLine()
                .AppendLine("We're aware it goes to streamlabs, however this is the best option we have available at this time.");
            
            return ReplyAsync(output.ToString());
        }
    }
}