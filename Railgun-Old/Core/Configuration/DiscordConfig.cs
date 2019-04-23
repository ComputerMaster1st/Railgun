using System.Collections.Generic;
using Newtonsoft.Json;
using Railgun.Core.Enums;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class DiscordConfig
    {
        public string Token { get; }
        public string Prefix { get; set; }
        public ulong MasterAdminId { get; }
        public ulong MasterGuildId { get; set; } = 0;
        public List<ulong> OtherAdmins { get; } = new List<ulong>();
        public BotLogChannels BotLogChannels { get; } = new BotLogChannels();

        public DiscordConfig(string token, string prefix, ulong masterAdminId) {
            Token = token;
            Prefix = prefix;
            MasterAdminId = masterAdminId;
        }

        [JsonConstructor]
        private DiscordConfig(string token, string prefix, ulong masterAdminId, ulong masterGuildId, BotLogChannels botLogChannels, List<ulong> otherAdmins) {
            Token = token;
            Prefix = prefix;
            MasterAdminId = masterAdminId;
            MasterGuildId = masterGuildId;
            BotLogChannels = botLogChannels;
            OtherAdmins = otherAdmins;
        }

        public void AssignMasterGuild(ulong guildId)
            => MasterGuildId = guildId;
        
        public bool AssignAdmin(ulong userId) {
            if (OtherAdmins.Contains(userId)) return false;

            OtherAdmins.Add(userId);
            return true;
        }

        public bool RemoveAdmin(ulong userId) {
            if (!OtherAdmins.Contains(userId)) return false;

            OtherAdmins.Remove(userId);
            return true;
        }

        public void AssignBotLogChannel(ulong channelId, BotLogType logType) {
            switch (logType) {
                case BotLogType.Common:
                    BotLogChannels.Common = channelId;
                    break;
                case BotLogType.CommandManager:
                    BotLogChannels.CommandMngr = channelId;
                    break;
                case BotLogType.GuildManager:
                    BotLogChannels.GuildMngr = channelId;
                    break;
                case BotLogType.MusicManager:
                    BotLogChannels.MusicMngr = channelId;
                    break;
                case BotLogType.MusicPlayerActive:
                    BotLogChannels.MusicPlayerActive = channelId;
                    break;
                case BotLogType.MusicPlayerError:
                    BotLogChannels.MusicPlayerError = channelId;
                    break;
                case BotLogType.AudioChord:
                    BotLogChannels.AudioChord = channelId;
                    break;
                case BotLogType.TaskScheduler:
                    BotLogChannels.TaskSch = channelId;
                    break;
                case BotLogType.TimerManager:
                    BotLogChannels.TimerMngr = channelId;
                    break;
            }
        }
    }
}