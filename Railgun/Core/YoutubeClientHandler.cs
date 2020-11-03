using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Railgun.Core
{
    public class YoutubeClientHandler : HttpClientHandler
    {
        private volatile Queue<(string Address, int Port)> _proxies = new Queue<(string Address, int Port)>();

        private void ParseProxies(string rawResponse)
        {
            var lines = rawResponse.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var tempList = new Queue<(string Address, int Port)>();

            foreach (var line in lines)
            {
                var proxy = line.Split(':', 2);
                        
                if (!_proxies.Any(x => x.Address == proxy[0]))
                    tempList.Enqueue((proxy[0], int.Parse(proxy[1])));
            }

            Interlocked.Exchange(ref _proxies, tempList);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            for (int i=5; i>0; i--)
            {
                if (_proxies.Count == 0)
                {
                    UseProxy = false;
                    var proxyRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.proxyscrape.com/?request=getproxies&proxytype=http&timeout=10000&country=all&ssl=no&anonymity=all");
                    var proxyResponse = await base.SendAsync(proxyRequest, cancellationToken);

                    if (proxyResponse.IsSuccessStatusCode) {
                        ParseProxies(await proxyResponse.Content.ReadAsStringAsync());
                        UseProxy = true;
                    }
                    else throw new HttpRequestException("Proxy Fetch Failure!");
                }

                var ytResponse = await base.SendAsync(request, cancellationToken);

                if (ytResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var newProxy = _proxies.Dequeue();
                    Proxy = new WebProxy(newProxy.Address, newProxy.Port);
                }
                else 
                    return ytResponse;
            }

            throw new HttpRequestException("Retry Count Exceeded!");
        }
    }
}