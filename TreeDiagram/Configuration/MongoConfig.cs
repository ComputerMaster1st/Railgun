namespace TreeDiagram.Configuration
{
    public class MongoConfig
    {
        public string Hostname { get; }
        public string Username { get; }
        public string Password { get; }

        public MongoConfig(string hostname, string username, string password)
        {
            Hostname = hostname;
            Username = username;
            Password = password;
        }
    }
}