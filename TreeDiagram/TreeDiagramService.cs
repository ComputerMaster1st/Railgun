using AudioChord;
using TreeDiagram.Configuration;

namespace TreeDiagram
{
    public class TreeDiagramService
    {
        private readonly TreeDiagramContext _treeDiagramContext;
        private readonly MusicService _musicService;
        
        public TreeDiagramService(PostgresConfig postgresConfig, MongoConfig mongoConfig)
        {
            _treeDiagramContext = new TreeDiagramContext(postgresConfig);
            
            _musicService = new MusicService(new MusicServiceConfig() {
                Username = mongoConfig.Username,
                Password = mongoConfig.Password,
                Hostname = mongoConfig.Hostname,
                EnableResync = true
            });
        }
        
        public TreeDiagramContext GetTreeDiagramContext()
            => _treeDiagramContext;

        public MusicService GetMusicService()
            => _musicService;
    }
}