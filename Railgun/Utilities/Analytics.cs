using Railgun.Core;
using Railgun.Core.Enums;
using Railgun.Core.Results;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Utilities
{
    public class Analytics
    {
        private readonly BotLog _botLog;

        public uint ReceivedMessages { get; set; } = 0;
        public uint UpdatedMessages { get; set; } = 0;
        public uint DeletedMessages { get; set; } = 0;
        public uint FilterDeletedMessages { get; set; } = 0;

        public Dictionary<string, int> UsedCommands { get; } = new Dictionary<string, int>();

        public Analytics(BotLog botLog) => _botLog = botLog;

        public Task ExecutedCommand(SystemContext ctx, CommandResult result)
        {
            var cmdString = result.CommandPath;
            var guild = ctx.Guild;

            if (UsedCommands.ContainsKey(cmdString)) UsedCommands[cmdString]++;
            else UsedCommands.Add(cmdString, 1);

            var output = new StringBuilder()
                .AppendFormat("<{0} <{1}>>", guild.Name, guild.Id).AppendLine()
                .AppendFormat("---- Command : {0}", cmdString).AppendLine();

            if (ctx.Message.Content.Length < 250)
                output.AppendFormat("---- Content : {0}", ctx.Message.Content).AppendLine();

            output.AppendLine("---- Result  : Completed");

            return _botLog.SendBotLogAsync(BotLogType.CommandManager, output.ToString());
        }
    }
}