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
        private readonly HttpClient _proxyClient = new HttpClient();

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
            //for (int i=5; i>0; i--)
            while (true)
            {
                if (_proxies.Count == 0)
                {
                    Console.WriteLine("Scraping for updated proxy server list...");
                    var proxyRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.proxyscrape.com/?request=getproxies&proxytype=http&timeout=10000&country=all&ssl=no&anonymity=all");
                    var proxyResponse = await _proxyClient.SendAsync(proxyRequest, cancellationToken);
                    Console.WriteLine("Fetch Complete");

                    if (proxyResponse.IsSuccessStatusCode)
                    {
                        ParseProxies(await proxyResponse.Content.ReadAsStringAsync());

                        if (!_proxies.TryDequeue(out (string Address, int Port) newProxy))
                            continue;

                        var proxy = Proxy as RotateProxy;
                        proxy.RotateAddress(newProxy.Address, newProxy.Port);
                        Console.WriteLine($"Found {_proxies.Count} Proxies!");
                        Console.WriteLine($"Now Using {proxy.Address.OriginalString}");
                    }
                    else throw new HttpRequestException("Proxy Fetch Failure!");
                }

                try
                {
                    using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                    {
                        var ytResponse = await base.SendAsync(request, cancellationTokenSource.Token);

                        if (ytResponse.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            if (!RotateProxy()) continue;
                        }
                        else
                        {
                            Console.WriteLine($"Successful response on proxy: {(Proxy as RotateProxy).Address.OriginalString}");
                            return ytResponse;
                        }
                            
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Timeout! Swapping Proxy...");
                    RotateProxy();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("HttpRequestException! Swapping Proxy...");
                    RotateProxy();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("OperationCanceledException! Swapping Proxy...");
                    RotateProxy();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            //throw new HttpRequestException("Retry Count Exceeded!");
        }

        private bool RotateProxy()
        {
            if (!_proxies.TryDequeue(out (string Address, int Port) newProxy))
                return false;

            var proxy = Proxy as RotateProxy;
            proxy.RotateAddress(newProxy.Address, newProxy.Port);

            var cookies = CookieContainer.GetCookies(new Uri("https://www.youtube.com"));

            foreach (Cookie cookie in cookies)
                cookie.Expired = true;

            Console.WriteLine($"Now Using {proxy.Address.OriginalString}");
            return true;
        }
    }
}