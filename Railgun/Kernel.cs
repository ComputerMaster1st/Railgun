using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using AudioChord;
using AudioChord.Caching.FileSystem;
using AudioChord.Extractors;
using AudioChord.Extractors.Discord;
using AudioChord.Metadata.Postgres;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Finite.Commands.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Apis.RandomCat;
using Railgun.Apis.Youtube;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Factories;
using Railgun.Core.Parsers;
using Railgun.Core.Pipelines;
using Railgun.Core.TypeReaders;
using Railgun.Events;
using Railgun.Events.OnMessageEvents;
using Railgun.Filters;
using Railgun.Inactivity;
using Railgun.Music;
using Railgun.Timers;
using Railgun.Utilities;
using TreeDiagram;
using YoutubeExplode;

namespace Railgun
{
    public class Kernel
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;

        private BotLog _botLog = null;
        private ServerCount _serverCount = null;
        private CommandService<SystemContext> _commandService;
        private EventLoader _eventLoader = null;
        private Analytics _analytics = null;
        private FilterLoader _filterLoader = null;
        private IServiceProvider _serviceProvider = null;
        private MusicServiceConfiguration _musicServiceConfig = null;
        private MusicService _musicService = null;
        private HttpClient _ytClient;
        //private YoutubeClientHandler _ytHandler;
        private HttpClientHandler _ytHandler;

        public Kernel(MasterConfig config, DiscordShardedClient client)
        {
            _config = config;
            _client = client;
            //_ytHandler = new YoutubeClientHandler();
            _ytHandler = new SimpleYoutubeClientHandler();

            _ytClient = new HttpClient(_ytHandler, true);
            _ytClient.DefaultRequestHeaders.Add("User-Agent", "Chrome/86.0.4240.111");
        }

        public void Boot()
        {
            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Command Service..."));

            _commandService = new CommandServiceBuilder<SystemContext>()
                .AddModules(Assembly.GetEntryAssembly())
                .AddTypeReaderFactory(() => new DiscordTypeReaderFactory()
                    .AddReader(new UserTypeReader())
                    .AddReader(new GuildUserTypeReader())
                    .AddReader(new TextChannelTypeReader())
                    .AddReader(new RoleTypeReader()))
                .AddPipeline<PrefixPipeline>()
                .AddCommandParser<SystemCommandParser<SystemContext>>()
                .AddPipeline<PreconditionPipeline>()
                .AddPipeline<FinalizePipeline>()
                .BuildCommandService();

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", $"Loaded {_commandService.GetAllCommands().Count()} Commands!"));

#if DEBUG
            DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
#endif

            var postgre = _config.PostgreSqlConfig;
            var mongo = _config.MongoDbConfig;
            var enricher = new MetaDataEnricher();

            _musicServiceConfig = new MusicServiceBuilder()
                .WithPostgresMetadataProvider($"Server={postgre.Hostname};Port=5432;Database=AudioChord;UserId={postgre.Username};Password={postgre.Password};")
#if DEBUG
                .WithCache(new FileSystemCache(dir.ToString()))
#else
                .WithCache(new FileSystemCache("/home/audiochord"))
#endif
                .WithExtractor<YouTubeExtractor>()
                .WithExtractor<DiscordExtractor>()
                .WithEnRicher(enricher)
                .Configure(f => {
                    f.Hostname = mongo.Hostname;
                    f.Username = mongo.Username;
                    f.Password = mongo.Password;
                    f.ExtractorConfiguration.ImportedHttpClient = _ytClient;
                })
                .Build();
            _musicService = new MusicService(_musicServiceConfig);
            _botLog = new BotLog(_config, _client);
            _serverCount = new ServerCount(_config, _client);
            _analytics = new Analytics(_botLog);

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Preparing Dependency Injection..."));
            
            _serviceProvider = new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton(_botLog)
                .AddSingleton(_analytics)
                .AddSingleton(_commandService)
                .AddSingleton(_musicServiceConfig)
                .AddSingleton(_musicService)
                .AddSingleton(_ytHandler)
                .AddSingleton<IDiscordClient>(_client)
                .AddSingleton<PlayerController>()
                .AddSingleton<YoutubeSearch>()
                .AddSingleton<TimerController>()
                .AddSingleton<InactivityController>()
                .AddSingleton<MusicController>()
                .AddDbContext<TreeDiagramContext>(options => {
                    options.UseNpgsql($"Server={postgre.Hostname};Port=5432;Database={postgre.Database};UserId={postgre.Username};Password={postgre.Password};")
                        .EnableSensitiveDataLogging()
                        .UseLazyLoadingProxies();
                    }, 
                    ServiceLifetime.Transient
                )
                .AddTransient<RandomCat>()
                .AddSingleton(enricher)
                .AddSingleton(new YoutubeClient(_ytClient))
                .BuildServiceProvider();

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Filters..."));

            _filterLoader = new FilterLoader(_serviceProvider)
                .AddMessageFilter<AntiCaps>()
                .AddMessageFilter<AntiInvite>()
                .AddMessageFilter<AntiUrl>();

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Events..."));

            _eventLoader = new EventLoader()
                .LoadEvent(new ConsoleLogEvent(_config, _client))
                .LoadEvent(new UnobservedEvent(_botLog))
                .LoadEvent(new OnMessageReceivedEvent(_client, _analytics, _serviceProvider)
                    .AddSubEvent(new OnInactivitySubEvent(_serviceProvider))
                    .AddSubEvent(new OnFilterSubEvent(_filterLoader, _analytics))
                    .AddSubEvent(new OnCommandSubEvent(_client, _commandService, _analytics, _botLog, _serviceProvider))
                )
                .LoadEvent(new OnMessageDeletedEvent(_client, _analytics))
                .LoadEvent(new OnGuildJoinEvent(_client, _botLog))
                .LoadEvent(new OnGuildLeaveEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnUserJoinEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnUserLeftEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnVoiceStateEvent(_client, _serviceProvider))
                .LoadEvent(new OnReadyEvent(_config, _client, _serverCount, _serviceProvider));

            _serverCount.Start();
        }
    }
}