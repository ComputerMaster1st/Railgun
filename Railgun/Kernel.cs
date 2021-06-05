using System;
using System.IO;
using System.Linq;
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
        private readonly HttpClient _ytClient;
        private readonly HttpClientHandler _ytHandler;
        private readonly BotLog _botLog;
        private readonly ServerCount _serverCount;
        private readonly Analytics _analytics;

        private CommandService<SystemContext> _commandService = null;
        private EventLoader _eventLoader = null;
        private FilterLoader _filterLoader = null;
        private IServiceProvider _serviceProvider = null;
        private MusicServiceConfiguration _musicServiceConfig = null;
        private MusicService _musicService = null;

        public Kernel(MasterConfig config, DiscordShardedClient client)
        {
            _config = config;
            _client = client;

            _botLog = new BotLog(_config, _client);
            _serverCount = new ServerCount(_config, _client);

            _ytHandler = new SimpleYoutubeClientHandler();
            _ytClient = new HttpClient(_ytHandler, true);
            _ytClient.DefaultRequestHeaders.Add("User-Agent", "Chrome/86.0.4240.111");

            _analytics = new Analytics(_botLog);
        }

        public void Boot()
        {
            #region Command Service

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Command Service..."));

            _commandService = new CommandServiceBuilder<SystemContext>()
                .AddModules(Assembly.GetEntryAssembly())
                .AddTypeReaderFactory(() => new DiscordTypeReaderFactory()
                    .AddReader<UserTypeReader>()
                    .AddReader<GuildUserTypeReader>()
                    .AddReader<TextChannelTypeReader>()
                    .AddReader<RoleTypeReader>())
                .AddPipeline<PrefixPipeline>()
                .AddCommandParser<SystemCommandParser<SystemContext>>()
                .AddPipeline<PreconditionPipeline>()
                .AddPipeline<FinalizePipeline>()
                .BuildCommandService();

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", $"Loaded {_commandService.GetAllCommands().Count()} Commands!"));

            #endregion

            #region Music Service (AudioChord)

            var postgre = _config.PostgreSqlConfig;
            var mongo = _config.MongoDbConfig;
            var enricher = new MetaDataEnricher();

#if DEBUG
            DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
#endif

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

            #endregion

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Preparing Dependency Injection..."));

            var collection = new ServiceCollection();

            #region Dependency Injection: Core

            collection.AddSingleton(_config)
                .AddSingleton(_client)
                .AddSingleton<IDiscordClient>(_client)
                .AddSingleton(_commandService)
                .AddSingleton(_botLog);

            #endregion

            #region Dependency Injection: Database

            collection.AddDbContext<TreeDiagramContext>(options =>
            {
                options.UseNpgsql($"Server={postgre.Hostname};Port=5432;Database={postgre.Database};UserId={postgre.Username};Password={postgre.Password};")
                    .EnableSensitiveDataLogging()
                    .UseLazyLoadingProxies();
            }, ServiceLifetime.Transient);

            #endregion

            #region Dependency Injection: Services

            collection.AddSingleton(_analytics)
                .AddSingleton(_musicServiceConfig)
                .AddSingleton(_musicService)
                .AddSingleton(_ytHandler)
                .AddSingleton(enricher)
                .AddSingleton<PlayerController>()
                .AddSingleton<TimerController>()
                .AddSingleton<InactivityController>()
                .AddSingleton<MusicController>();

            #endregion

            #region Dependency Injection: API

            collection.AddSingleton<YoutubeSearch>()
                .AddTransient<RandomCat>()
                .AddSingleton(new YoutubeClient(_ytClient));

            #endregion

            _serviceProvider = collection.BuildServiceProvider();

            #region Dependency Injection: Filters

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Filters..."));

            _filterLoader = new FilterLoader(_serviceProvider)
                .AddMessageFilter<AntiCaps>()
                .AddMessageFilter<AntiInvite>()
                .AddMessageFilter<AntiUrl>();

            #endregion

            #region Dependency Injection: Events

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Kernel", "Loading Events..."));

            _eventLoader = new EventLoader()
                .LoadEvent(new ConsoleLogEvent(_config, _client))
                .LoadEvent(new UnobservedEvent(_botLog))
                .LoadEvent(new OnMessageDeletedEvent(_client, _analytics))
                .LoadEvent(new OnGuildJoinEvent(_client, _botLog))
                .LoadEvent(new OnGuildLeaveEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnUserJoinEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnUserLeftEvent(_client, _botLog, _serviceProvider))
                .LoadEvent(new OnVoiceStateEvent(_client, _serviceProvider))
                .LoadEvent(new OnReadyEvent(_config, _client, _serverCount, _serviceProvider));

            #endregion

            #region Dependency Injection: Message Handling

            _eventLoader.
                LoadEvent(new OnMessageReceivedEvent(_client, _analytics, _serviceProvider)
                    .AddSubEvent(new OnInactivitySubEvent(_serviceProvider))
                    .AddSubEvent(new OnFilterSubEvent(_filterLoader, _analytics))
                    .AddSubEvent(new OnCommandSubEvent(_client, _commandService, _analytics, _botLog, _serviceProvider))
                );

            #endregion

            _serverCount.Start();
        }
    }
}