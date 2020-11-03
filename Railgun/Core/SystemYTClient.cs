using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Railgun.Core
{
    public class SystemYTClient
    {
        private readonly HttpClientHandler _handler;
        private readonly HttpClient _proxyFetcher;
        private readonly List<(string Address, string Port, bool Is429)> _proxies = new List<(string Address, string Port, bool Is429)>();

        private string _currentProxyAddress = null;

        public HttpClient YTClient { get; }

        public SystemYTClient()
        {
            _handler = new HttpClientHandler
            {
                UseProxy = true
            };

            if (_handler.SupportsAutomaticDecompression)
                _handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            YTClient = new HttpClient(_handler, true);
            YTClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.111 Safari/537.36");
        }

        public async Task<int> FetchProxiesAsync()
        {
            var result = await _proxyFetcher.GetStringAsync("https://api.proxyscrape.com/?request=getproxies&proxytype=http&timeout=10000&country=all&ssl=no&anonymity=all");
            var lines = result.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var proxy = line.Split(':', 2);
                
                if (!_proxies.Any(x => x.Address == proxy[0]))
                {
                    _proxies.Add((proxy[0], proxy[1], false));
                }
            }

            return _proxies.Count;
        }

        public bool RotateProxy()
        {
            if (_proxies.Count < 1 || _proxies.All(x => x.Is429 == true))
                return false;

            var proxy = _proxies.First(x => x.Is429 == false);

            _currentProxyAddress = proxy.Address;
            _handler.Proxy = new WebProxy(proxy.Address, int.Parse(proxy.Port));

            return true;
        }

        public bool Set429ed()
        {
            var proxy = _proxies.First(x => x.Address == _currentProxyAddress);
            proxy.Is429 = true;

            return RotateProxy();
        }
    }
}