using Discord;
using Railgun.Core;
using Railgun.Core.Enums;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Events
{
    public class UnobservedEvent : IEvent
    {
        private BotLog _botLog;

        public UnobservedEvent(BotLog botLog) => _botLog = botLog;

        public void Load() => TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Task.Run(() => ExecuteAsync(e)).ConfigureAwait(false);
        };

        private Task ExecuteAsync(UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Error, "System", "An unobserved task threw an exception!", e.Exception));

            var output = new StringBuilder()
                .AppendLine("An unobserved task threw an exception!")
                .AppendLine(e.Exception.InnerException.ToString());

            return _botLog.SendBotLogAsync(BotLogType.TaskScheduler, output.ToString());
        }
    }
}