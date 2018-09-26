Imports AudioChord
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Utilities
Imports TreeDiagram

Namespace Core
    
    Public Class Events
    
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        Private ReadOnly _commandUtils As CommandUtils
        Private ReadOnly _serverCount As ServerCount

        Private ReadOnly _client As DiscordShardedClient
        
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _musicService As MusicService
        
        Private ReadOnly _playerManager As PlayerManager
        
        Private _initialized As Boolean = False
        Private ReadOnly _shardsReady As New Dictionary(Of Integer, Boolean)
    End Class
    
End NameSpace
'
'namespace Railgun.Core
'{
'    public class Events
'    {

'        private TimerManager timerManager;
'
'        public Events(MasterConfig config, DiscordShardedClient client, Log log, CommandUtils commandUtils, ServerCount serverCount, MusicService musicService, PlayerManager playerManager, TimerManager timerManager, VManager<GFD_Bite> vgBite, VManager<GFD_RST> vgRst, VManager<GD_Command> vgCommand, VManager<GD_JoinLeave> vgJoinLeave, VManager<GD_Music> vgMusic, VManager<GD_Mention> vgMention, VManager<GD_Prefix> vgPrefix, VManager<GD_Warn> vgWarn, VManager<UD_Mention> vuMention, VManager<FD_AntiCaps> vfAntiCaps, VManager<FD_AntiUrl> vfAntiUrl) {
'            client.JoinedGuild += JoinedGuildAsync;
'            client.LeftGuild += LeftGuildAsync;
'            client.UserJoined += UserJoinedAsync;
'            client.UserLeft += UserLeftAsync;
'            client.UserVoiceStateUpdated += UserVoiceUpdatedAsync;
'            client.ShardReady += ShardReadyAsync;
'
'            this.config = config;
'            this.client = client;
'            this.log = log;
'            this.commandUtils = commandUtils;
'            this.serverCount = serverCount;
'            this.musicService = musicService;
'            this.timerManager = timerManager;
'            this.playerManager = playerManager;
'            this.vgBite = vgBite;
'            this.vgRst = vgRst;
'            this.vgCommand = vgCommand;
'            this.vgJoinLeave = vgJoinLeave;
'            this.vgMention = vgMention;
'            this.vgMusic = vgMusic;
'            this.vgPrefix = vgPrefix;
'            this.vgWarn = vgWarn;
'            this.vuMention = vuMention;
'            this.vfAntiCaps = vfAntiCaps;
'            this.vfAntiUrl = vfAntiUrl;
'        }
'
'        private async Task JoinedGuildAsync(SocketGuild sGuild)
'            => await log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Joined", BotLogType.GuildManager);
'
'        private async Task LeftGuildAsync(SocketGuild sGuild)
'            => await Task.Factory.StartNew(async () => await DedicatedLeftGuildTask(sGuild));
'
'        private async Task DedicatedLeftGuildTask(SocketGuild sGuild) {
'            if (playerManager.IsCreated(sGuild.Id)) playerManager.GetPlayer(sGuild.Id).CancelStream();
'
'            GFD_Bite vBite = await vgBite.GetAsync(sGuild.Id);
'            GFD_RST vRst = await vgRst.GetAsync(sGuild.Id);
'            GD_Command vCommand = await vgCommand.GetAsync(sGuild.Id);
'            GD_JoinLeave vJoinLeave = await vgJoinLeave.GetAsync(sGuild.Id);
'            GD_Music vMusic = await vgMusic.GetAsync(sGuild.Id);
'            GD_Mention vMention = await vgMention.GetAsync(sGuild.Id);
'            GD_Prefix vPrefix = await vgPrefix.GetAsync(sGuild.Id);
'            GD_Warn vWarn = await vgWarn.GetAsync(sGuild.Id);    
'            FD_AntiCaps vAntiCaps = await vfAntiCaps.GetAsync(sGuild.Id);
'            FD_AntiUrl vAntiUrl = await vfAntiUrl.GetAsync(sGuild.Id);
'
'            await musicService.CancelGuildMusicProcessingAsync(sGuild.Id);
'
'            if (vBite != null) await vBite.DeleteAsync();
'            if (vRst != null) await vRst.DeleteAsync();
'            if (vCommand != null) await vCommand.DeleteAsync();
'            if (vJoinLeave != null) await vJoinLeave.DeleteAsync();
'            if (vMention != null) await vMention.DeleteAsync();
'            if (vMusic != null) await vMusic.DeleteAsync();            
'            if (vPrefix != null) await vPrefix.DeleteAsync();
'            if (vWarn != null) await vWarn.DeleteAsync();
'            if (vAntiCaps != null) await vAntiCaps.DeleteAsync();
'            if (vAntiUrl != null) await vAntiUrl.DeleteAsync();
'
'            await log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Left", BotLogType.GuildManager);
'        }
'
'        private async Task UserJoinedAsync(SocketGuildUser sUser) {
'            IGuildUser user = sUser;
'            GD_JoinLeave gJoinLeave = await vgJoinLeave.GetAsync(user.GuildId);
'
'            if (gJoinLeave == null) return;
'
'            GD_Mention gMention = await vgMention.GetAsync(user.GuildId);
'            UD_Mention uMention = await vuMention.GetAsync(user.Id);
'            string notification = gJoinLeave.GetMessage(MsgType.Join).Replace("<server>", user.Guild.Name);
'
'            notification = notification.Replace("<user>", await commandUtils.GetUsernameOrMentionAsync(user));
'            await SendJoinLeaveMessageAsync(gJoinLeave, user, notification);            
'        }
'
'        private async Task UserLeftAsync(SocketGuildUser sUser) {
'            IGuildUser user = sUser;
'            GD_JoinLeave gJoinLeave = await vgJoinLeave.GetAsync(user.GuildId);
'
'            if (gJoinLeave == null) return;
'
'            string notification = gJoinLeave.GetMessage(MsgType.Leave);
'
'            if (!string.IsNullOrEmpty(notification)) notification = notification.Replace("<user>", user.Username);
'            else return;
'            
'            await SendJoinLeaveMessageAsync(gJoinLeave, user, notification);      
'        }
'
'        private async Task SendJoinLeaveMessageAsync(GD_JoinLeave gJoinLeave, IGuildUser user, string notification) {
'            if (string.IsNullOrEmpty(notification))
'                return;
'            else if (gJoinLeave.SendAsDm)
'                try {
'                    IDMChannel dm = await user.GetOrCreateDMChannelAsync();
'                    await dm.SendMessageAsync(notification);
'                } catch { }                
'            else if (gJoinLeave.JoinLeaveChannelId != 0) {
'                IGuild guild = user.Guild;
'                ITextChannel tc = await guild.GetTextChannelAsync(gJoinLeave.JoinLeaveChannelId);
'
'                if (tc != null) await tc.SendMessageAsync(notification);
'            }
'        }
'
'        private async Task UserVoiceUpdatedAsync(SocketUser sUser, SocketVoiceState before, SocketVoiceState after) {
'            if (sUser.IsBot || sUser.IsWebhook) return;
'
'            if (after.VoiceChannel == null) return;
'
'            IGuild guild = after.VoiceChannel.Guild;
'            IGuildUser user = await guild.GetUserAsync(sUser.Id);
'
'            if (playerManager.IsCreated(guild.Id) || user.VoiceChannel == null) 
'                return;
'
'            GD_Music gMusic = await vgMusic.GetAsync(guild.Id);
'
'            if (gMusic == null) return;
'
'            IVoiceChannel vc = user.VoiceChannel;
'            ITextChannel tc = (gMusic.AutoTextChannel != 0) ? await guild.GetTextChannelAsync(gMusic.AutoTextChannel) : null;
'
'            if (vc.Id == gMusic.AutoVoiceChannel && tc != null)
'                await playerManager.CreatePlayerAsync(user, vc, tc, true);
'        }
'
'        private async Task ShardReadyAsync(DiscordSocketClient sClient) {
'            if (playerManager.ActivePlayers.Count > 0)
'                foreach (var player in playerManager.ActivePlayers)
'                    player.Value.Item2.CancelStream();
'            
'            if (!shardsReady.ContainsKey(sClient.ShardId)) shardsReady.Add(sClient.ShardId, false);
'            else shardsReady[sClient.ShardId] = true;
'
'            await log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, $"SHARD {sClient.ShardId}", $"Shard {((shardsReady[sClient.ShardId]) ? "Re-" : "")}Connected! ({sClient.Guilds.Count} Servers)"));
'            await timerManager.Initialize();
'            
'            if (initialized) return;
'            else if (shardsReady.Count < client.Shards.Count) return;
'
'            initialized = true;
'            serverCount.PreviousGuildCount = client.Guilds.Count;
'            
'            await client.SetGameAsync($"{config.Prefix}help || {client.Guilds.Count} Servers!", type:ActivityType.Watching);
'        }
'    }
'}