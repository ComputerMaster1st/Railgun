using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Railgun.Core.Enums;

namespace Railgun.Core.Configuration
{
    [JsonObject(MemberSerialization.Fields)]
    public class MasterConfig
    {
        [JsonIgnore]
        private const string Filename = "masterconfig.json";
        [JsonIgnore]
        public string YoutubeDirectory { get; } = "/";
        [JsonIgnore]
        public string YoutubeDomain { get; } = "youtube.com";

        public DiscordConfig DiscordConfig { get; private set; }
        public DatabaseConfig PostgreSqlConfig { get; private set; }
        public DatabaseConfig MongoDbConfig { get; private set; }

        public Dictionary<string, string> YoutubeCookies { get; private set; } = new Dictionary<string, string>();

        public string GoogleApiToken { get; private set; }
        public string RandomCatApiToken { get; private set; }

        public MasterConfig(DiscordConfig discordConfig, DatabaseConfig postgresql, DatabaseConfig mongodb, string googleApiToken, string randomCatApiKey)
        {
            DiscordConfig = discordConfig;
            PostgreSqlConfig = postgresql;
            MongoDbConfig = mongodb;
            GoogleApiToken = googleApiToken;
            RandomCatApiToken = randomCatApiKey;
        }

        [JsonConstructor]
        private MasterConfig() {}

        private static string SetupInput(string query)
        {
            Console.Write(query);
            return Console.ReadLine();
        }

        private static string SetupInput(string query, string defaultTo)
        {
            Console.Write(query);
            var output = Console.ReadLine();
            return string.IsNullOrWhiteSpace(output) ? defaultTo: output;
        }

        public static MasterConfig Load() 
        {
            if (!File.Exists(Filename)) return Setup();
            return JsonConvert.DeserializeObject<MasterConfig>(File.ReadAllText(Filename));
        }

        private static MasterConfig Setup()
        {
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
            masterConfig.Save();

            return masterConfig;
        }

        private void Save()
            => File.WriteAllText(Filename, JsonConvert.SerializeObject(this, Formatting.Indented));
        
        public void AssignMasterGuild(ulong guildId)
        {
            DiscordConfig.AssignMasterGuild(guildId);
            Save();
        }

        public void AssignBotLogChannel(ulong channelId, BotLogType logType)
        {
            DiscordConfig.AssignBotLogChannel(channelId, logType);
            Save();
        }

        public void AssignPrefix(string prefix)
        {
            DiscordConfig.Prefix = prefix;
            Save();
        }

        public bool AssignAdmin(ulong userId)
        {
            if (!DiscordConfig.AssignAdmin(userId)) return false;
            Save();
            return true;
        }

        public bool RemoveAdmin(ulong userId)
        {
            if (!DiscordConfig.RemoveAdmin(userId)) return false;
            Save();
            return true;
        }

        public void UpdateYoutubeCookies(IEnumerable<Cookie> cookies)
        {
            var newCookies = new Dictionary<string, string>();

            foreach (var cookie in cookies)
            {
                if (newCookies.ContainsKey(cookie.Name))
                {
                    newCookies[cookie.Name] = cookie.Value;
                    continue;
                }

                newCookies.Add(cookie.Name, cookie.Value);
            }

            YoutubeCookies = newCookies;
            Save();
        }
    }
}