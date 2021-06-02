using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("botlog")]
        public class RootBotLog : SystemBase
        {
            private readonly MasterConfig _config;

            public RootBotLog(MasterConfig config)
                => _config = config;

            [Command]
            public Task ExecuteAsync(string logType, ITextChannel textChannel)
            {
                Enum.TryParse(logType, out BotLogType botLogType);

                _config.AssignBotLogChannel(textChannel.Id, botLogType);

                return ReplyAsync(string.Format("The channel {0} has been set as the {1} botlog channel.", 
                    textChannel.Mention, 
                    botLogType));
            }

            [Command]
            public Task ExecuteAsync(string logType)
                => ExecuteAsync(logType, Context.Channel as ITextChannel);

            [Command]
            public Task ExecuteAsync()
            {
                var values = Enum.GetValues(typeof(BotLogType))
                    .Cast<BotLogType>();

                var output = new StringBuilder()
                    .AppendLine("All Bot Log Channels:")
                    .AppendLine();

                foreach (var value in values)
                    output.AppendLine(value.ToString());

                return ReplyAsync(output.ToString());
            }
        }
    }
}