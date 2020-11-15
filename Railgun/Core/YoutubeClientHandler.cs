using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Railgun.Core
{
    public class YoutubeClientHandler : HttpClientHandler
    {
        private ConcurrentQueue<(string Address, int Port)> _proxies = new ConcurrentQueue<(string Address, int Port)>();
        private readonly RotateProxy _rotateProxy = new RotateProxy();
        private readonly HttpClient _proxyClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(3) };

        public YoutubeClientHandler()
        {
            if (SupportsAutomaticDecompression)
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            Proxy = _rotateProxy;
            UseProxy = true;
        }

        private void ParseProxies(string rawResponse)
        {
            var lines = rawResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var tempList = new ConcurrentQueue<(string Address, int Port)>();

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
            var retry = 2;

            while (true)
            {
                if (_proxies.Count == 0)
                {
                    if (retry == 0)
                        throw new HttpRequestException("Proxy Rotator Retry Count Exceeded!");
                    if (retry > 0)
                        retry--;

                    Console.WriteLine("Scraping for updated proxy server list...");
                    try
                    {
                        var proxyRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.proxy-list.download/api/v1/get?type=http");
                        var proxyResponse = await _proxyClient.SendAsync(proxyRequest);
                            
                        Console.WriteLine("Fetch Complete");

                        if (proxyResponse.IsSuccessStatusCode)
                        {
                            ParseProxies(await proxyResponse.Content.ReadAsStringAsync());

                            if (_proxies.Count < 1) continue;

                            Console.WriteLine($"Found {_proxies.Count} Proxies!");

                            if (!await RotateProxyAsync()) continue;
                        }
                        else throw new HttpRequestException("Proxy Fetch Failure!");
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message.Contains("Proxy Fetch Failure!"))
                        {
                            throw ex;
                        }
                    }
                    catch { continue; }
                }

                try
                {
                    Console.WriteLine($"Request Uri: {request.RequestUri.OriginalString}");
                    var ytResponse = await base.SendAsync(new HttpRequestMessage(HttpMethod.Get, request.RequestUri), cancellationToken);

                    if (ytResponse.IsSuccessStatusCode)
                    {
                        return ytResponse;
                    }
                    if (!await RotateProxyAsync()) continue;
                }
                catch (Exception) { await RotateProxyAsync(); }
            }
        }

        private async Task<bool> RotateProxyAsync()
        {
            RotateProxy proxy;

            while (true)
            {
                if (!_proxies.TryDequeue(out (string Address, int Port) newProxy))
                    return false;

                proxy = Proxy as RotateProxy;
                proxy.RotateAddress(newProxy.Address, newProxy.Port);

                try
                {
                    Console.WriteLine($"Pinging/Tracing {proxy.Address}");
                    var pingMsg = new HttpRequestMessage(HttpMethod.Get, proxy.Address);
                    var pingResponse = await _proxyClient.SendAsync(pingMsg);

                    Console.WriteLine($"Proxy Response From {proxy.Address}");
                    break;
                }
                catch { }
            }

            var cookies = CookieContainer.GetCookies(new Uri("https://youtube.com"));

            foreach (Cookie cookie in cookies)
                cookie.Expired = true;

            Console.WriteLine($"Now Using {proxy.Address.OriginalString}");
            return true;
        }
    }
}