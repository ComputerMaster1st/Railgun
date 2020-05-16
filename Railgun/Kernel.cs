using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using AudioChord;
using AudioChord.Caching.FileSystem;
using AudioChord.Extractors;
using AudioChord.Extractors.Discord;
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
        private HttpClientHandler _youtubeHttpClientHandler = null;
        private HttpClient _youtubehttpClient = null;

        public Kernel(MasterConfig config, DiscordShardedClient client)
        {
            _config = config;
            _client = client;

            CreateYoutubeHttpClient();
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

            var postgre = _config.PostgreSqlConfig;
            var mongo = _config.MongoDbConfig;
            var enricher = new MetaDataEnricher();
            _musicServiceConfig = new MusicServiceConfiguration() {
                Hostname = mongo.Hostname,
                Username = mongo.Username,
                Password = mongo.Password,
                SongCacheFactory = () => new FileSystemCache("/home/audiochord"),
                Extractors = () => new List<IAudioExtractor>() { new DiscordExtractor(), new YouTubeExtractor(_youtubehttpClient) },
                Enrichers = () => new List<IAudioMetadataEnricher> { enricher }
            };
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
                .AddSingleton(_youtubeHttpClientHandler)
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
                .AddSingleton(new YoutubeClient(_youtubehttpClient))
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

        private void CreateYoutubeHttpClient()
        {
            _youtubeHttpClientHandler = new HttpClientHandler() { CookieContainer = new CookieContainer() };
            
            if (_youtubeHttpClientHandler.SupportsAutomaticDecompression)
                _youtubeHttpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            var container = _youtubeHttpClientHandler.CookieContainer;

            // Use LOGIN_INFO, SAPISID, APISID, SSID, HSID, SID, VISITOR_INFO1_LIVE, PREF, YSC
            foreach (var keyvaluepair in _config.YoutubeCookies)
                container.Add(new Cookie(keyvaluepair.Key, keyvaluepair.Value, _config.YoutubeDirectory, _config.YoutubeDomain));

            _youtubeHttpClientHandler.UseCookies = true;

            _youtubehttpClient = new HttpClient(_youtubeHttpClientHandler, true);
            _youtubehttpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.163 Safari/537.36");
        }
    }
}