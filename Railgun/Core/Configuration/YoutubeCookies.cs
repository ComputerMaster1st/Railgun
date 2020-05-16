using Newtonsoft.Json;

namespace Railgun.Core.Configuration
{

    [JsonObject(MemberSerialization.Fields)]
    public class YoutubeCookies
    {
        public const string CookieDirectory = "/";
        public const string CookieDomain = "youtube.com";

        public string Login_Info { get; } = string.Empty;

        public string SAPISID { get; } = string.Empty;
        public string APISID { get; } = string.Empty;
        public string SSID { get; } = string.Empty;
        public string HSID { get; } = string.Empty;
        public string SID { get; } = string.Empty;
        public string VISITOR_INFO1_LIVE { get; } = string.Empty;
        public string PREF { get; } = string.Empty;
        public string YSC { get; } = string.Empty;
    }
}