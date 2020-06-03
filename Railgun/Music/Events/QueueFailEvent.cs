using System.Text;
using System.Threading.Tasks;
using Discord;
using Railgun.Core;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Music.PlayerEventArgs;

namespace Railgun.Music.Events
{
    public class QueueFailEvent : IPlayerEvent
    {
        private readonly PlayerController _controller;
        private readonly BotLog _botLog;
        private PlayerContainer _container;

        public QueueFailEvent(PlayerController controller, BotLog botLog)
        {
            _controller = controller;
            _botLog = botLog;
        }

        public void Load(PlayerContainer container) 
        {
            _container = container;
            _container.Player.QueueFailed += async (s, a) => await ExecuteAsync(a);
        }

        private async Task ExecuteAsync(QueueFailEventArgs args)
        {
			var tc = _container.TextChannel;

			SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Error, "Music", $"{tc.GuildId} Exception!", args.Exception));

			var logOutput = new StringBuilder()
				.AppendFormat("<{0} ({1})> Music Player Error!", tc.Guild.Name, tc.GuildId).AppendLine()
				.AppendFormat("---- Error : {0}", args.Exception.ToString());

			await _botLog.SendBotLogAsync(BotLogType.MusicPlayerError, logOutput.ToString());
        }
    }
}