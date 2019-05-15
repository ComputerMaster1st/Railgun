using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Railgun.Core.Configuration;

namespace Railgun.Apis.RandomCat
{
    public class RandomCat
    {
        private const string BaseUrl = "http://thecatapi.com/api/images/";
        private readonly string _apiKey;

        public RandomCat(MasterConfig config)
            => _apiKey = config.RandomCatApiToken;

        public async Task<Stream> GetRandomCatAsync()
        {
            Stream stream;

            using (var client = new HttpClient())
            {
                stream = await client.GetStreamAsync($"{BaseUrl}get?api_key={_apiKey}&type=png&size=med");
            }

            return stream;
        }
    }
}