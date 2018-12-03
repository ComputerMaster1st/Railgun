using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Railgun.Core.Configuration;

namespace Railgun.Core.Api
{
    public class RandomCat
    {
        private const string BaseUrl = "http://thecatapi.com/api/images/";
        private readonly HttpClient _client = new HttpClient();
        private readonly string _apiKey;

        public RandomCat(MasterConfig config)
            => _apiKey = config.RandomCatApiToken;

        public async Task<Stream> GetRandomCatAsync()
            => await _client.GetStreamAsync($"{BaseUrl}get?api_key={_apiKey}&type=png&size=med");
    }
}