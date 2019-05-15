using System.Text;
using System.Threading.Tasks;
using Discord;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;

namespace Railgun.Music.Events
{
    public class FinishedEvent : IPlayerEvent
    {
        private readonly PlayerController _controller;
        private readonly BotLog _botLog;
        private PlayerContainer _container;

        public FinishedEvent(PlayerController controller, BotLog botLog)
        {
            _controller = controller;
            _botLog = botLog;
        }

        public void Load(PlayerContainer container) 
        {
            _container = container;
            _container.Player.Finished += async (s, a) => await ExecuteAsync(a);
        }

        private async Task ExecuteAsync(FinishedEventArgs args)
        {
			var tc = _container.TextChannel;

			try 
            {
				var output = new StringBuilder();

				if (args.Exception != null) 
                {
					SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Error, "Music", $"{tc.GuildId} Exception!", args.Exception));

					var logOutput = new StringBuilder()
						.AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine()
						.AppendFormat("---- Error : {0}", args.Exception.ToString());

					await _botLog.SendBotLogAsync(BotLogType.MusicPlayerError, logOutput.ToString());

					output.AppendLine("An error has occured while playing! The stream has been automatically reset. You may start playing music again at any time.");
				}

				var autoOutput = args.AutoDisconnected ? "Auto-" : "";

				output.AppendFormat("{0}Left Voice Channel", autoOutput);

				await tc.SendMessageAsync(output.ToString());
			} 
            catch 
            {
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
				await _botLog.SendBotLogAsync(BotLogType.MusicPlayerError, $"<{tc.Guild.Name} ({args.GuildId})> Crash-Disconnected");
			} 
            finally 
            {
				await _container.LogEntry.DeleteAsync();
				await _controller.StopPlayerAsync(args.GuildId, args.AutoDisconnected);
			}
        }
    }
}