using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Timers;
using Railgun.Timers.Containers;
using TreeDiagram;

namespace Railgun.Commands
{
	[Alias("remindme")]
	public class RemindMe : SystemBase
	{
		private readonly TimerController _timerController;

		public RemindMe(TimerController timerController)
			=> _timerController = timerController;

		[Command]
		public Task ExecuteAsync(string expireIn, [Remainder] string message)
		{
			if (string.IsNullOrWhiteSpace(message))
				return ReplyAsync("You didn't specify a message to remind you about.");

			if (message.Length > 1800)
				return ReplyAsync("Remind me has a character limit of 1800. Please shorten the message.");

			var times = expireIn.Split(':');
			var invalidFormat = new StringBuilder()
				.AppendFormat("The time format given is {0}! Please use one of the following formats...", 
					Format.Bold("INVALID")).AppendLine()
				.AppendLine()
				.AppendFormat("{0} << Minutes {1} 10 Minutes Example : {2}", 
					Format.Code("m"), 
					SystemUtilities.GetSeparator, 
					Format.Code("10 this is my msg.")).AppendLine()
				.AppendFormat("{0} << Hours:Minutes {1} 1 Hour, 10 Minutes Example : {2}", 
					Format.Code("h:m"), 
					SystemUtilities.GetSeparator, 
					Format.Code("1:10 this is my msg.")).AppendLine()
				.AppendFormat("{0} << Days:Hours:Minutes {1} 1 Day, 1 Hour, 10 Minutes Example : {2}", 
					Format.Code("d:h:m"), 
					SystemUtilities.GetSeparator, 
					Format.Code("1:1:10 this is my msg."));

			if (times.Length > 3)
				return ReplyAsync(invalidFormat.ToString());

			var dhm = new[] { 0, 0, 0 };
			var i = dhm.Length - times.Length;
			var ti = 0;

			while (i < dhm.Length)
			{
				if (!int.TryParse(times[ti], out int number)) 
					return ReplyAsync(invalidFormat.ToString());

				dhm[i] = number;
				i++;
				ti++;
			}

			var expireTime = DateTime.UtcNow.AddDays(dhm[0]).AddHours(dhm[1]).AddMinutes(dhm[2]);

			if (expireTime < DateTime.UtcNow) 
			{
				var output = new StringBuilder()
					.AppendFormat("{0}, you asked me to {1} remind you about the following...", 
						Context.Author.Mention, 
						Format.Bold("INSTANTLY")).AppendLine()
					.AppendLine()
					.AppendLine(message);

				return ReplyAsync(output.ToString());
			}

			var data = Context.Database.TimerRemindMes.CreateTimer(Context.Guild.Id, expireTime);

			data.TextChannelId = Context.Channel.Id;
			data.UserId = Context.Author.Id;
			data.Message = message;

			_timerController.CreateAndStartTimer<RemindMeTimerContainer>(data);

			return ReplyAsync(string.Format("Reminder has been created! You'll be pinged here at {0} UTC.",
				Format.Bold(data.TimerExpire.ToString(CultureInfo.CurrentCulture))));
		}
	}
}