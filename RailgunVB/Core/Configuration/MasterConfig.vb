Imports System.IO
Imports Newtonsoft.Json
Imports RailgunVB.Core.Logging

Namespace Core.Configuration
    
    <JsonObject(MemberSerialization.Fields)>
    Public Class MasterConfig
        <JsonIgnore> Private Const Filename As String = "masterconfig.json"
        
        Public ReadOnly Property DiscordConfig As DiscordConfig
        Public ReadOnly Property DatabaseConfig As DatabaseConfig
        
        Public ReadOnly Property GoogleApiToken As String
        Public ReadOnly Property RandomCatApiToken As String

        <JsonConstructor>
        Public Sub New (
            discordConfig As DiscordConfig,
            databaseConfig As DatabaseConfig,
            googleApiToken As String,
            randomCatApiToken As String
        )
            Me.DiscordConfig = discordConfig
            Me.DatabaseConfig = databaseConfig
            Me.GoogleApiToken = googleApiToken
            Me.RandomCatApiToken = randomCatApiToken
        End Sub
        
        Public Shared Async Function LoadAsync() As Task(Of MasterConfig)
            If Not (File.Exists(Filename)) Then Return Await SetupAsync()
            
            Dim json As String = Await File.ReadAllTextAsync(Filename)
            Return JsonConvert.DeserializeObject(Of MasterConfig)(json)
        End Function
        
        Private Shared Async Function SetupAsync() As Task(Of MasterConfig)
            ' <===== DISCORD CONFIGURATION SETUP =====>
            Console.Write("Discord || Token: ")
            Dim token As String = Console.ReadLine()

            Console.Write("Discord || Prefix [!]: ")
            Dim rawPrefix As String = Console.ReadLine()
            Dim prefix As String = If(String.IsNullOrWhiteSpace(rawPrefix), "!")

            Console.Write("Discord || Master Admin ID: ")
            Dim masterAdminId As ULong = ULong.Parse(Console.ReadLine())
            
            Dim discordConfig As New DiscordConfig(token, prefix, masterAdminId)
            ' <=======================================>
            
            ' <===== DATABASE CONFIGURATION SETUP =====>
            Console.WriteLine("Database || Hostname [localhost]: ")
            Dim rawHostname as String = Console.ReadLine()
            Dim hostname As String = If(String.IsNullOrWhiteSpace(rawHostname), "localhost")
            
            Console.WriteLine("Database || Username: ")
            Dim username As String = Console.ReadLine()
            
            Console.WriteLine("Database || Password: ")
            Dim password As String = Console.ReadLine()
            
            Console.WriteLine("Database || Database: ")
            Dim database As String = Console.ReadLine()
            
            Dim databaseConfig as New DatabaseConfig(hostname, username, password, database)
            ' <========================================>

            Console.Write("Other || Google API Key: ")
            Dim googleApiKey As String = Console.ReadLine()

            Console.Write("Other || RandomCat API Key: ")
            Dim randomCatApiKey As String = Console.ReadLine()

            Dim masterConfig As New MasterConfig(discordConfig, databaseConfig, googleApiKey, randomCatApiKey)
            await masterConfig.SaveAsync()
            Return masterConfig
        End Function
        
        Private Async Function SaveAsync() As Task
            Await File.WriteAllTextAsync(Filename, JsonConvert.SerializeObject(Me, Formatting.Indented))
        End Function
        
        Public Async Function AssignAsMasterGuildAsync(guildId As ULong) As Task
            _DiscordConfig.AssignMasterGuild(guildId)
            Await SaveAsync()
        End Function
        
        Public Async Function AssignBotLogChannelAsync(channelId As ULong, botLogType As BotLogType) As Task
            _DiscordConfig.AssignBotLogChannel(channelId, botLogType)
            Await SaveAsync()
        End Function
        
        Public Async Function AssignAdminAsync(userId As ULong) As Task(Of Boolean)
            If Not (_DiscordConfig.AssignAdmin(userId)) Then Return False
            
            Await SaveAsync()
            Return True
        End Function
        
        Public Async Function RemoveAdminAsync(userId As ULong) As Task(Of Boolean)
            If Not (_DiscordConfig.RemoveAdmin(userId)) Then Return False
            
            Await SaveAsync()
            Return True
        End Function
        
    End Class
    
End Namespace
