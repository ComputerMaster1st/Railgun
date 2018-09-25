using Microsoft.EntityFrameworkCore;
using TreeDiagram.Configuration;

namespace TreeDiagram
{
    public class TreeDiagramContext : DbContext
    {
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _data;

        private const int Port = 5432;

        internal TreeDiagramContext(PostgresConfig config)
        {
            _host = config.Hostname;
            _user = config.Username;
            _pass = config.Password;
            _data = config.Database;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Server={_host};Port={Port};Database={_data};UserId={_user};Password={_pass};");
            base.OnConfiguring(optionsBuilder);
        }
    }
}