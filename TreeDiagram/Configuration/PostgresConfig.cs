namespace TreeDiagram.Configuration
{
    public class PostgresConfig
    {
        public string Hostname { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Database { get; private set; }
    }
}