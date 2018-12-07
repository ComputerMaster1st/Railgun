using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Managers;
using TreeDiagram;

namespace Railgun.Commands.Utilities
{
    [Alias("remindme")]
    public class RemindMe : SystemBase
    {
        private readonly TimerManager _timerManager;

        public RemindMe(TimerManager timerManager) => _timerManager = timerManager;

        [Command]
        public async Task RemindMeAsync(string expireIn, [Remainder] string message) {
            if (string.IsNullOrWhiteSpace(message)) {
                await ReplyAsync("You didn't specify a message to remind you about.");
                return;
            } else if (message.Length > 1800) {
                await ReplyAsync("Remind me has a character limit of 1800. Please shorten the message.");
                return;
            }

            var times = expireIn.Split(':');
            var invalidFormat = new StringBuilder()
                .AppendFormat("The time format given is {0}! Please use one of the following formats...", Format.Bold("INVALID")).AppendLine()
                .AppendLine()
                .AppendFormat("{0} << Minutes || 10 Minutes Example : {1}", Format.Code("m"), Format.Code("10 this is my msg.")).AppendLine()
                .AppendFormat("{0} << Hours:Minutes || 1 Hour, 10 Minutes Example : {1}", Format.Code("h:m"), Format.Code("1:10 this is my msg.")).AppendLine()
                .AppendFormat("{0} << Days:Hours:Minutes || 1 Day, 1 Hour, 10 Minutes Example : {1}", Format.Code("d:h:m"), Format.Code("1:1:10 this is my msg."));

            if (times.Length > 3) {
                await ReplyAsync(invalidFormat.ToString());
                return;
            }

            var dhm = new int[] { 0, 0, 0 };
            var i = dhm.Length - times.Length;
            var ti = 0;

            while (i < dhm.Length) {
                if (!int.TryParse(times[ti], out int number)) {
                    await ReplyAsync(invalidFormat.ToString());
                    return;
                }

                dhm[i] = number;
                i++;
                ti++;
            }

            var expireTime = DateTime.UtcNow.AddDays(dhm[0]).AddHours(dhm[1]).AddMinutes(dhm[2]);

            if (expireTime < DateTime.UtcNow) {
                var output = new StringBuilder()
                    .AppendFormat("{0}, you asked me to {1} remind you about the following...", Context.Author.Mention, Format.Bold("INSTANTLY")).AppendLine()
                    .AppendLine()
                    .AppendLine(message);
                
                await ReplyAsync(output.ToString());
                return;
            }

            var data = await Context.Database.TimerRemindMes.CreateTimerAsync();

            data.GuildId = Context.Guild.Id;
            data.TextChannelId = Context.Channel.Id;
            data.UserId = Context.Author.Id;
            data.Message = message;
            data.TimerExpire = expireTime;

            await _timerManager.CreateAndStartRemindMeContainerAsync(data, true);
            await ReplyAsync($"Reminder has been created! You'll be pinged here at {Format.Bold(data.TimerExpire.ToString())} UTC.");
        }
    }
}