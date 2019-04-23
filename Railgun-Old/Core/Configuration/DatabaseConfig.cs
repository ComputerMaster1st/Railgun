using Newtonsoft.Json;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class DatabaseConfig
    {
        public string Hostname { get; }
        public string Username { get; }
        public string Password { get; }
        public string Database { get; }

        [JsonConstructor]
        public DatabaseConfig(string hostname, string username, string password, string database) {
            Hostname = hostname;
            Username = username;
            Password = password;
            Database = database;
        }
    }
}