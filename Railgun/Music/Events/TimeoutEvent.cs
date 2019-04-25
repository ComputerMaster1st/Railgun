using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Core.Enums;
using Railgun.Events;
using Railgun.Music.PlayerEventArgs;

namespace Railgun.Music.Events
{
    public class TimeoutEvent : IPlayerEvent
    {
		private readonly BotLog _botLog;
        private PlayerContainer _container;

        public TimeoutEvent(BotLog botLog) => _botLog = botLog;

        public void Load(PlayerContainer container)
		{
			_container = container;
			_container.Player.Timeout += (s, a) => Task.Factory.StartNew(() => ExecuteAsync(a));
		}

        private async Task ExecuteAsync(TimeoutEventArgs args)
        {
			await _container.Lock.WaitAsync();
			var tc = _container.TextChannel;

			try {
				await tc.SendMessageAsync("Connection to Discord Voice has timed out! Please try again.");
			} catch {
				SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, "Music", $"{args.GuildId} Missing TC!"));
			} finally {
				var output = new StringBuilder()
					.AppendFormat("<{0} ({1})> Action Timeout!", tc.Guild.Name, args.GuildId).AppendLine()
					.AppendFormat("---- Exception : {0}", args.Exception.ToString());

				await _botLog.SendBotLogAsync(BotLogType.MusicPlayerError, output.ToString());
			}

			_container.Lock.Release();
        }
    }
}