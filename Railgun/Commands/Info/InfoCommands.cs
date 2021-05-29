using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Utilities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("commands")]
        public class InfoCommands : SystemBase
        {
            private readonly Analytics _analytics;

            public InfoCommands(Analytics anaytics)
                => _analytics = anaytics;

            [Command]
            public Task ExecuteAsync()
            {
                var commands = _analytics.UsedCommands.OrderByDescending(r => r.Value);
                var count = 20;
                var output = new StringBuilder()
                    .AppendFormat("Railgun Top {0} Command Analytics:", count).AppendLine().AppendLine();

                foreach (var command in commands)
                {
                    output.AppendFormat("{0} <= {1}", command.Value, command.Key).AppendLine();
                    count--;

                    if (count < 1) break;
                }

                return ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
