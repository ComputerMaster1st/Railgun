using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Enums;
using TreeDiagram;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos;
using Playlist = AudioChord.Playlist;

namespace Railgun.Music
{
    public class MusicController
    {
        private readonly BotLog _botLog;
        private readonly MusicService _musicService;
        private readonly MusicServiceConfiguration _musicConfig;
        private readonly IServiceProvider _services;

        public MusicController(BotLog botLog, MusicService musicService, MusicServiceConfiguration musicConfig, IServiceProvider services)
		{
            _botLog = botLog;
            _musicService = musicService;
            _musicConfig = musicConfig;
            _services = services;
		}

        private async Task<Playlist> GetPlaylistAsync(ITextChannel tc)
        {
            Playlist playlist;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                playlist = await SystemUtilities.GetPlaylistAsync(_musicService, db.ServerMusics.GetOrCreateData(tc.GuildId));

                await db.SaveChangesAsync();
            }

            return playlist;
        }

        private string BuildMsgOutput(int installed, int imported, int invalidUrls, int needEncoding)
        {
            var output = new StringBuilder()
                .AppendLine(Format.Bold(Format.Underline("Processing Completed!")))
                .AppendLine()
                .AppendFormat("{0} - Already Installed", Format.Code($"[{installed}]")).AppendLine()
                .AppendFormat("{0} - Imported From Repository", Format.Code($"[{imported}]")).AppendLine();

            if (invalidUrls > 0) output.AppendFormat("{0} - Invalid Urls", Format.Code($"[{invalidUrls}]")).AppendLine();
            if (needEncoding > 0) output.AppendLine().AppendFormat("{0} - Need Checking/Downloading", Format.Code($"[{needEncoding}]")).AppendLine()
                .AppendLine(Format.Italics("Music that require checking/downloading will be done when the player requests it. Expect playback to have a short delayed for these songs."));

            return output.ToString();
        }

        private async Task<(int InvalidUrls, int Installed, int Imported, int NeedEncoding)> ProcessSongIdsAsync(IEnumerable<string> urls, ITextChannel tc)
        {
            var playlist = await GetPlaylistAsync(tc);
            var playlistModified = false;
            var invalidUrls = 0;
            var installed = 0;
            var imported = 0;
            var needEncoding = 0;
            var videoIds = new List<string>();

            foreach (var url in urls)
            {
                var cleanUrl = url.Trim(' ', '<', '>');
                var videoId = VideoId.TryParse(url);

                if (videoId == null)
                {
                    invalidUrls++;
                    await tc.SendMessageAsync($"{Format.Bold("Invalid Url :")} {Format.EscapeUrl(url)}");
                    continue;
                }

                videoIds.Add(videoId);
            }

            foreach (var videoId in videoIds)
            {
                var songId = new SongId("YOUTUBE", videoId);
                var song = await _musicService.TryGetSongAsync(songId);
                if (song.Item1)
                {
                    if (playlist.Songs.Contains(song.Item2.Id)) installed++;
                    else
                    {
                        playlist.Songs.Add(song.Item2.Id);
                        playlistModified = true;
                        imported++;
                    }

                    continue;
                }

                playlist.Songs.Add(songId);
                playlistModified = true;
                needEncoding++;
            }

            if (playlistModified) await _musicService.Playlist.UpdateAsync(playlist);

            return (invalidUrls, installed, imported, needEncoding);
        }

        public async Task AddYoutubeSongsAsync(IEnumerable<string> urls, ITextChannel tc)
		{
            await tc.SendMessageAsync($"Processing {Format.Bold(urls.Count().ToString())} song(s)... This may take some time depending on how many songs there are.");

            var (InvalidUrls, Installed, Imported, NeedEncoding) = await ProcessSongIdsAsync(urls, tc);

            await Task.Delay(1000);
			await tc.SendMessageAsync(BuildMsgOutput(Installed, Imported, InvalidUrls, NeedEncoding));
		}

        public async Task ProcessYoutubePlaylistAsync(string playlistUrl, ITextChannel tc)
        {
            var playlistId = YoutubeExplode.Playlists.PlaylistId.TryParse(playlistUrl);

            if (playlistId == null)
            {
                await tc.SendMessageAsync("Invalid Playlist Url! Please double-check it.");
                return;
            }

            var startedAt = DateTime.Now;
            var client = new YoutubeClient();
            IReadOnlyList<Video> youtubePlaylist;

            try
            {
                youtubePlaylist = await client.Playlists.GetVideosAsync(playlistId.Value);
            }
            catch (RequestLimitExceededException)
            {
                await tc.SendMessageAsync($"Unable to process playlist! Youtube Rate-Limited (Error Code: 429)! Please try again later.");
                return;
            }

            await tc.SendMessageAsync($"Found {Format.Bold(youtubePlaylist.Count().ToString())} songs! Now processing...");

            var urls = new List<string>();
            foreach (var video in youtubePlaylist)
                if (video.Duration <= _musicConfig.ExtractorConfiguration.MaxSongDuration)
                    urls.Add(video.Url);

            var (InvalidUrls, Installed, Imported, NeedEncoding) = await ProcessSongIdsAsync(urls, tc);

            await Task.Delay(1000);
            await tc.SendMessageAsync(BuildMsgOutput(Installed, Imported, InvalidUrls, NeedEncoding));

            var logOutput = new StringBuilder()
                .AppendFormat("<{0} <{1}>> YouTube Playlist Processed!", tc.Guild.Name, tc.GuildId).AppendLine()
                .AppendFormat("---- Url                : {0}", playlistUrl).AppendLine()
                .AppendFormat("---- Started            : {0}", startedAt).AppendLine()
                .AppendFormat("---- Finished           : {0}", DateTime.Now).AppendLine()
                .AppendLine()
                .AppendFormat("---- Already Installed  : {0}", Installed).AppendLine()
                .AppendFormat("---- Imported From Repo : {0}", Imported).AppendLine();

            if (InvalidUrls > 0) logOutput.AppendFormat("---- Invalid Urls       : {0}", InvalidUrls).AppendLine();
            if (NeedEncoding > 0) logOutput.AppendFormat("---- Require Encoding   : {0}", NeedEncoding).AppendLine();

            await _botLog.SendBotLogAsync(BotLogType.AudioChord, logOutput.ToString());
        }
    }
}