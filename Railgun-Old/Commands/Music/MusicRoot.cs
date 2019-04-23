using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("root"), BotAdmin]
        public class MusicRoot : SystemBase
        {
            private readonly PlayerManager _playerManager;

            public MusicRoot(PlayerManager playerManager) => _playerManager = playerManager;
            
            [Command("active"), BotPerms(ChannelPermission.AttachFiles)]
            public async Task ActiveAsync() {
                if (_playerManager.PlayerContainers.Count < 1) {
                    await ReplyAsync("There are no active music streams at this time.");

                    return;
                }
                
                var output = new StringBuilder()
                    .AppendFormat("Active Music Streams ({0} Total):", _playerManager.PlayerContainers.Count).AppendLine().AppendLine();
                
                foreach (var info in _playerManager.PlayerContainers) {
                    var player = info.Player;
                    var song = player.GetFirstSongRequest();
                    
                    output.AppendFormat("Id : {0} {1} Spawned At : {2} {1} Status : {3}", info.GuildId, Response.GetSeparator(), player.CreatedAt, player.Status).AppendLine()
                        .AppendFormat("\\--> Latency : {0}ms {1} Playing : {2} {1} Since : {3}", player.Latency, Response.GetSeparator(), song == null ? "Searching..." : song.Id.ToString(), player.SongStartedAt).AppendLine().AppendLine();
                }
                
                if (output.Length < 1950) {
                    await ReplyAsync(Format.Code(output.ToString()));

                    return;
                }

                await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "ActivePlayers.txt", output.ToString(), includeGuildName:false);
            }
            
            [Command("kill")]
            public Task KillAsync(ulong id) {
                _playerManager.DisconnectPlayer(id);

                return ReplyAsync($"Sent 'Kill Code' to Player ID {id}.");
            }
        }
    }
}