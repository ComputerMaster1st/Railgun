using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core.Configuration;
using Railgun.Core.Utilities;

namespace Railgun.Core.Logging
{
    public class Log
    {
        private const string LogDirectory = "logs";

        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;

        public Log(MasterConfig config, DiscordShardedClient client) {
            _config = config;
            _client = client;

            if (!Directory.Exists(LogDirectory)) Directory.CreateDirectory(LogDirectory);

            _client.Log += LogToConsoleAsync;
        }

        private async Task WriteToLogFileAsync(string entry) {
            using (var logfile = File.AppendText($"{LogDirectory}/{DateTime.Today.ToString("yyyy-MM-dd")}.log")) {
                await logfile.WriteLineAsync(entry);
            }
        }

        private async Task SendBotLogAsync(IGuild guild, BotLogType logType, ulong logChannelId, string entry, bool pingMaster) {
            var output = new StringBuilder()
                .AppendFormat("[ {0} ] || {1} {2}", 
                    DateTime.Now.ToString("HH:mm:ss"),
                    logType != BotLogType.Common ? $"{logType} ||" : "",
                    entry);

            try {
                ITextChannel tc;

                if (logChannelId != 0) tc = await guild.GetTextChannelAsync(logChannelId);
                else if (_config.DiscordConfig.BotLogChannels.Common != 0)
                    tc = await guild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.Common);
                else return;

                var pingMasterStr = pingMaster ? $"<@!{_config.DiscordConfig.MasterAdminId}>": string.Empty;

                if (output.Length > 1950) {
                    await CommandUtils.SendStringAsFileAsync(tc, "UnobservedTask.log", $"Error! Refer to file {pingMasterStr}!", includeGuildName:false);
                    
                    return;
                }

                await tc.SendMessageAsync(Format.Code(output.ToString()) + pingMasterStr);
            } catch { }

            await WriteToLogFileAsync(output.ToString());
        }

        public async Task LogToBotLogAsync(string entry, BotLogType logType, bool pingMaster = false) {
            if (_config.DiscordConfig.MasterGuildId == 0) return;

            var guild = await ((IDiscordClient)_client).GetGuildAsync(_config.DiscordConfig.MasterGuildId);

            switch (logType) {
                case BotLogType.Common:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.Common, entry, pingMaster);
                    break;
                case BotLogType.AudioChord:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.AudioChord, entry, pingMaster);
                    break;
                case BotLogType.CommandManager:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.CommandMngr, entry, pingMaster);
                    break;
                case BotLogType.GuildManager:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.GuildMngr, entry, pingMaster);
                    break;
                case BotLogType.MusicManager:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.MusicMngr, entry, pingMaster);
                    break;
                case BotLogType.MusicPlayer:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.MusicPlayer, entry, pingMaster);
                    break;
                case BotLogType.TaskScheduler:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.TaskSch, entry, pingMaster);
                    break;
                case BotLogType.TimerManager:
                    await SendBotLogAsync(guild, logType, _config.DiscordConfig.BotLogChannels.TimerMngr, entry, pingMaster);
                    break;
            }
        }

        public async Task LogToConsoleAsync(LogMessage message) {
            switch (message.Severity) {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }

            Console.WriteLine(message.ToString());

            await WriteToLogFileAsync(message.ToString());
        }
    }
}