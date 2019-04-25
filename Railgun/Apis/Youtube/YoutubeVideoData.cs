namespace Railgun.Apis.Youtube
{
    public class YoutubeVideoData
    {
        public string Id { get; }
        public string Name { get; }
        public string Uploader { get; }
        
        internal YoutubeVideoData(string name, string id, string uploader)
        {
            Id = id;
            Name = name;
            Uploader = uploader;
        }
    }
}