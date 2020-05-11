using System;
using System.Collections.Generic;
using System.Linq;
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

        public Kernel(MasterConfig config, DiscordShardedClient client)
        {
            _config = config;
            _client = client;
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
            var discordEnricher = new DiscordMetaDataEnricher();
            var youtubeEncricher = new YoutubeMetaDataEnricher();
            _musicServiceConfig = new MusicServiceConfiguration() {
                Hostname = mongo.Hostname,
                Username = mongo.Username,
                Password = mongo.Password,
                SongCacheFactory = () => new FileSystemCache("/home/audiochord"),
                Extractors = () => new List<IAudioExtractor>() { new DiscordExtractor(), new YouTubeExtractor() },
                Enrichers = () => new List<IAudioMetadataEnricher> { discordEnricher, youtubeEncricher }
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
                .AddSingleton(discordEnricher)
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