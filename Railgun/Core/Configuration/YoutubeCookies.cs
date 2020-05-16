using Newtonsoft.Json;

namespace Railgun.Core.Configuration
{

    [JsonObject(MemberSerialization.Fields)]
    public class YoutubeCookies
    {
        [JsonIgnore]
        public string Directory { get; } = "/";
        [JsonIgnore]
        public string Domain { get; } = "youtube.com";

        public string LOGIN_INFO { get; set; } = string.Empty;

        public string SAPISID { get; set; } = string.Empty;
        public string APISID { get; set; } = string.Empty;
        public string SSID { get; set; } = string.Empty;
        public string HSID { get; set; } = string.Empty;
        public string SID { get; set; } = string.Empty;
        public string VISITOR_INFO1_LIVE { get; set; } = string.Empty;
        public string PREF { get; set; } = string.Empty;
        public string YSC { get; set; } = string.Empty;
    }
}