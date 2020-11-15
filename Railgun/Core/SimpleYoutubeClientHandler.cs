using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Railgun.Core
{
    public class SimpleYoutubeClientHandler : HttpClientHandler
    {

        public SimpleYoutubeClientHandler()
        {
            if (SupportsAutomaticDecompression)
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookies = CookieContainer.GetCookies(new Uri("https://youtube.com"));

            foreach (Cookie cookie in cookies)
                cookie.Expired = true;
            
            return base.SendAsync(request, cancellationToken);
        }
    }
}