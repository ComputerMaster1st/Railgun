using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Railgun.Core.Logging;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class MasterConfig
    {
        [JsonIgnore]
        private const string Filename = "masterconfig.json";

        public DiscordConfig DiscordConfig { get; }
        public DatabaseConfig PostgreSqlConfig { get; }
        public DatabaseConfig MongoDbConfig { get; }

        public string GoogleApiToken { get; }
        public string RandomCatApiToken { get; }

        [JsonConstructor]
        public MasterConfig(DiscordConfig discordConfig, DatabaseConfig postgresql, DatabaseConfig mongodb, string googleApiToken, string randomCatApiKey) {
            DiscordConfig = discordConfig;
            PostgreSqlConfig = postgresql;
            MongoDbConfig = mongodb;
            GoogleApiToken = googleApiToken;
            RandomCatApiToken = randomCatApiKey;
        }

        private static string SetupInput(string query) {
            Console.Write(query);

            return Console.ReadLine();
        }

        private static string SetupInput(string query, string defaultTo) {
            Console.Write(query);

            var output = Console.ReadLine();

            return string.IsNullOrWhiteSpace(output) ? defaultTo: output;
        }

        public static async Task<MasterConfig> LoadAsync() {
            if (!File.Exists(Filename)) return await SetupAsync();

            var json = await File.ReadAllTextAsync(Filename);

            return JsonConvert.DeserializeObject<MasterConfig>(json);
        }

        private static async Task<MasterConfig> SetupAsync() {
            var token = SetupInput("Discord || Token: ");
            var prefix = SetupInput("Discord || Prefix [!]: ", "!");
            var masterAdminId = ulong.Parse(SetupInput("Discord || Master Admin ID: "));
            var discordConfig = new DiscordConfig(token, prefix, masterAdminId);

            var phostname = SetupInput("Database || Postgre || Hostname [localhost]: ", "localhost");
            var pusername = SetupInput("Database || Postgre || Username: ");
            var ppassword = SetupInput("Database || Postgre || Password: ");
            var pdatabase = SetupInput("Database || Postgre || Database: ");
            var postgresqlConfig = new DatabaseConfig(phostname, pusername, ppassword, pdatabase);

            var mhostname = SetupInput("Database || Mongo || Hostname [localhost]: ", "localhost");
            var musername = SetupInput("Database || Mongo || Username: ");
            var mpassword = SetupInput("Database || Mongo || Password: ");
            var mongodbConfig = new DatabaseConfig(mhostname, musername, mpassword, null);

            var googleApiKey = SetupInput("Other || Google API Key: ");
            var randomCatApiKey = SetupInput("Other || RandomCat API Key: ");

            var masterConfig = new MasterConfig(discordConfig, postgresqlConfig, mongodbConfig, googleApiKey, randomCatApiKey);

            await masterConfig.SaveAsync();

            return masterConfig;
        }

        private Task SaveAsync()
            => File.WriteAllTextAsync(Filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        
        public Task AssignMasterGuildAsync(ulong guildId) {
            DiscordConfig.AssignMasterGuild(guildId);

            return SaveAsync();
        }

        public Task AssignBotLogChannelAsync(ulong channelId, BotLogType logType) {
            DiscordConfig.AssignBotLogChannel(channelId, logType);

            return SaveAsync();
        }

        public async Task AssignPrefixAsync(string prefix) {
            DiscordConfig.Prefix = prefix;

            await SaveAsync();
        }

        public async Task<bool> AssignAdminAsync(ulong userId) {
            if (!DiscordConfig.AssignAdmin(userId)) return false;

            await SaveAsync();

            return true;
        }

        public async Task<bool> RemoveAdminAsync(ulong userId) {
            if (!DiscordConfig.RemoveAdmin(userId)) return false;

            await SaveAsync();

            return true;
        }
    }
}