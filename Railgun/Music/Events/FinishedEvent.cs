using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Music.Events
{
    public class FinishedEvent : IPlayerEvent
    {
        private readonly PlayerController _controller;
        private readonly BotLog _botLog;
        private PlayerContainer _container;
        private readonly IServiceProvider _services;

        public FinishedEvent(PlayerController controller, BotLog botLog, IServiceProvider services)
        {
            _controller = controller;
            _botLog = botLog;
            _services = services;
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
                ServerMusic data;

                using (var scope = _services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                    data = db.ServerMusics.GetData(tc.GuildId);
                }

                if (data != null && data.SilentNowPlaying && args.Exception == null) return;

                var output = new StringBuilder();
                if (args.Exception != null)
                {
                    SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Error, "Music", $"{tc.GuildId} Exception!", args.Exception));

                    var logOutput = new StringBuilder()
                        .AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine()
                        .AppendFormat("---- Error : {0}", args.Exception.ToString());

                    await _botLog.SendBotLogAsync(BotLogType.MusicPlayerError, logOutput.ToString());

                    output.AppendLine("An error has occured while playing! The stream has been automatically reset. You may start playing music again at any time.")
                        .AppendFormat("{0} {1}", Format.Bold("ERROR:"), args.Exception.Message).AppendLine();
                }

				var autoOutput = args.Reason != DisconnectReason.Manual ? "Auto-" : "";

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
				await _controller.StopPlayerAsync(args.GuildId, true);
			}
        }
    }
}