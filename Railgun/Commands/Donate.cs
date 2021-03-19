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
        public Task DonateAsync() {
            var output = new StringBuilder()
                .AppendLine("Railgun is hosted on a dedicated server which is paid monthly and we never ask for a dime. However, if you still wish to donate to us, feel free to use the following URL.")
                .AppendFormat("Our PayPal Donation Link: {0}", Format.EscapeUrl("https://www.paypal.com/donate?hosted_button_id=XGQZX7JHUC56C")).AppendLine();
            
            return ReplyAsync(output.ToString());
        }
    }
}