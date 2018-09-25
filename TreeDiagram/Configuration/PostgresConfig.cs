namespace TreeDiagram.Configuration
{
    public class PostgresConfig
    {
        public string Hostname { get; }
        public string Username { get; }
        public string Password { get; }
        public string Database { get; }

        public PostgresConfig(string hostname, string username, string password, string database)
        {
            Hostname = hostname;
            Username = username;
            Password = password;
            Database = database;
        }
    }
}