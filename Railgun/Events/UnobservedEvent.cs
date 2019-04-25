using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Railgun.Core;
using Railgun.Core.Enums;

namespace Railgun.Events
{
    public class UnobservedEvent : IEvent
    {
        private BotLog _botLog;

        public UnobservedEvent(BotLog botLog) => _botLog = botLog;

        public void Load() => TaskScheduler.UnobservedTaskException += (s, e) => Task.Factory.StartNew(() => ExecuteAsync(e));

        private Task ExecuteAsync(UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Error, "System", "An unobserved task threw an exception!", e.Exception));

            var output = new StringBuilder()
                .AppendLine("An unobserved task threw an exception!")
                .AppendLine(e.Exception.InnerException.ToString());
            
            _botLog.SendBotLogAsync(BotLogType.TaskScheduler, output.ToString()).GetAwaiter();
            return Task.CompletedTask;
        }
    }
}