using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Finite.Commands.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Api;
using Railgun.Core.Api.Youtube;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Pipelines;
using Railgun.Core.Commands.Readers;
using Railgun.Core.Configuration;
using Railgun.Core.Filters;
using Railgun.Core.Logging;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Core
{
    public class Initializer
    {
        private readonly MasterConfig _masterConfig;
        private readonly DiscordShardedClient _client;
        private CommandService<SystemContext> _commandService;
        private IServiceProvider _services;
        private Log _log;
        private ServerCount _serverCount;

        public Initializer(MasterConfig config, DiscordShardedClient client) {
            _masterConfig = config;
            _client = client;
         
            _log = new Log(_masterConfig, _client);
            _serverCount = new ServerCount(_masterConfig, _client);
        }

        public async Task InitializeAsync() {
            var postgreConfig = _masterConfig.PostgreSqlConfig;
            var mongoConfig = _masterConfig.MongoDbConfig;

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "Loading Commands..."));

            _commandService = new CommandServiceBuilder<SystemContext>()
                .AddModules(Assembly.GetEntryAssembly())
                .AddTypeReaderFactory<DiscordTypeReaderFactory>(() => {
                    return new DiscordTypeReaderFactory()
                        .AddReader(new IUserTypeReader())
                        .AddReader(new IGuildUserTypeReader())
                        .AddReader(new ITextChannelTypeReader())
                        .AddReader(new IRoleTypeReader());
                })
                .AddPipeline<PrefixPipeline>()
                .AddCommandParser<SystemCommandParser<SystemContext>>()
                .AddPipeline<PreconditionPipeline>()
                .AddPipeline<FinalizePipeline>()
                .BuildCommandService();

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", $"{_commandService.GetAllCommands().Count()} Commands Loaded!"));
            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "Preparing Dependency Injection..."));

            _services = new ServiceCollection()
                .AddSingleton(_masterConfig)
                .AddSingleton(_client)
                .AddSingleton<IDiscordClient>(_client)
                .AddSingleton(_commandService)
                .AddSingleton(_log)
                .AddSingleton(_serverCount)
                .AddDbContext<TreeDiagramContext>(options => {
                    options.UseNpgsql(
                        string.Format("Server={0};Port=5432;Database={1};UserId={2};Password={3};",
                            postgreConfig.Hostname,
                            postgreConfig.Database,
                            postgreConfig.Username,
                            postgreConfig.Password
                        )
                    ).EnableSensitiveDataLogging().UseLazyLoadingProxies();
                }, ServiceLifetime.Transient)
                .AddSingleton(new MusicService(new MusicServiceConfiguration() {
                    Hostname = mongoConfig.Hostname,
                    Username = mongoConfig.Username,
                    Password = mongoConfig.Password,
                    EnableResync = true
                }))
                .AddSingleton<Analytics>()
                .AddTransient<CommandUtils>()
                .AddSingleton<Events>()
                .AddTransient<RandomCat>()
                .AddSingleton<CommandManager>()
                .AddSingleton<FilterManager>()
                .AddSingleton<MusicManager>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<TimerManager>()
                .AddSingleton<AntiCaps>()
                .AddSingleton<AntiUrl>()
                .AddSingleton<YoutubeSearch>()
                .BuildServiceProvider();
            
            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "Dependency Injection Ready!"));
            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "Performing Startup..."));
            
            TaskScheduler.UnobservedTaskException += async (sender, e) => await UnobservedTaskAsync(e);

            _services.GetService<Analytics>();
            _services.GetService<Events>();
            _services.GetService<CommandManager>();
            _services.GetService<AntiCaps>();
            _services.GetService<AntiUrl>();
        }

        private async Task UnobservedTaskAsync(UnobservedTaskExceptionEventArgs e) {
            e.SetObserved();

            await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "System", "An unobserved task threw an exception!", e.Exception));

            var output = new StringBuilder()
                .AppendLine("An unobserved task threw an exception!")
                .AppendLine(e.Exception.InnerException.ToString());
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.TaskScheduler);
        }
    }
}