using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Railgun.Core.Configuration;

namespace Railgun.Apis.Youtube
{
    public class YoutubeSearch
    {
        private readonly YouTubeService _service;

        public YoutubeSearch(MasterConfig config) => _service = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = config.GoogleApiToken,
            ApplicationName = this.GetType().ToString()
        });

        public async Task<YoutubeVideoData> GetVideoAsync(string query)
        {
            var request = _service.Search.List("snippet");

            request.Q = query;
            request.MaxResults = 1;

            var response = await request.ExecuteAsync();

            if (response.Items.Count < 1) return null;

            var result = response.Items.FirstOrDefault();

            if (result == null || !(result.Id.Kind == "youtube#video")) return null;

            var snippet = result.Snippet;

            return new YoutubeVideoData(snippet.Title, result.Id.VideoId, snippet.ChannelTitle);
        }

        public async Task<IEnumerable<YoutubeVideoData>> GetVideosAsync(string playlistId)
        {
            var list = new List<YoutubeVideoData>();

            var nextPageToken = "";
            while (nextPageToken != null)
            {
                var request = _service.PlaylistItems.List("snippet");
                request.Id = playlistId;
                request.MaxResults = 50;
                request.PageToken = nextPageToken;

                var response = await request.ExecuteAsync();

                if (response.Items.Count < 1) return null;

                foreach (var item in response.Items)
                    list.Add(new YoutubeVideoData(item.Snippet.Title, item.ContentDetails.VideoId, item.Snippet.ChannelTitle));

                nextPageToken = response.NextPageToken;
            }

            return list;
        }
    }
}