Imports System.IO
Imports Newtonsoft.Json
Imports RailgunVB.Core.Logging

Namespace Core.Configuration
    
    <JsonObject(MemberSerialization.Fields)>
    Public Class MasterConfig
        <JsonIgnore> Private Const Filename As String = "masterconfig.json"
        
        Public ReadOnly Property DiscordConfig As DiscordConfig
        Public ReadOnly Property PostgreDatabaseConfig As DatabaseConfig
        Public ReadOnly Property MongoDatabaseConfig As DatabaseConfig
        
        Public ReadOnly Property GoogleApiToken As String
        Public ReadOnly Property RandomCatApiToken As String

        <JsonConstructor>
        Public Sub New (
            discordConfig As DiscordConfig,
            postgreDatabaseConfig As DatabaseConfig,
            mongoDatabaseConfig As DatabaseConfig,
            googleApiToken As String,
            randomCatApiToken As String
        )
            Me.DiscordConfig = discordConfig
            Me.PostgreDatabaseConfig = postgreDatabaseConfig
            Me.MongoDatabaseConfig = mongoDatabaseConfig
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
            Dim token As String = SetupInput("Discord || Token: ")
            Dim prefix As String = SetupInput("Discord || Prefix [!]: ", "!")
            Dim masterAdminId As ULong = ULong.Parse(SetupInput("Discord || Master Admin ID: "))
            
            Dim discordConfig As New DiscordConfig(token, prefix, masterAdminId)
            ' <=======================================>
            
            ' <===== POSTGRE DATABASE CONFIGURATION SETUP =====>
            Dim phostname As String = SetupInput("Database || Postgre || Hostname [localhost]: ", "localhost")
            Dim pusername As String = SetupInput("Database || Postgre || Username: ")
            Dim ppassword As String = SetupInput("Database || Postgre || Password: ")
            Dim pdatabase As String = SetupInput("Database || Postgre || Database: ")
            
            Dim postgreDatabaseConfig as New DatabaseConfig(phostname, pusername, ppassword, pdatabase)
            ' <==============================================>
            
            ' <===== MONGO DATABASE CONFIGURATION SETUP =====>
            Dim mhostname As String = SetupInput("Database || Mongo || Hostname [localhost]: ", "localhost")
            Dim musername As String = SetupInput("Database || Mongo || Username: ")
            Dim mpassword As String = SetupInput("Database || Mongo || Password: ")
            
            Dim mongoDatabaseConfig as New DatabaseConfig(mhostname, musername, mpassword, Nothing)
            ' <==============================================>

            Dim googleApiKey As String = SetupInput("Other || Google API Key: ")
            Dim randomCatApiKey As String = SetupInput("Other || RandomCat API Key: ")

            Dim masterConfig As New MasterConfig(discordConfig, postgreDatabaseConfig, mongoDatabaseConfig, googleApiKey, randomCatApiKey)
            await masterConfig.SaveAsync()
            Return masterConfig
        End Function
        
        Private Shared Function SetupInput(query As String) As String
            Console.Write(query)
            Return Console.ReadLine()
        End Function
        
        Private Shared Function SetupInput(query As String, defaultTo As String) As String
            Console.Write(query)
            Dim output As String = Console.ReadLine()
            Return If(String.IsNullOrWhiteSpace(query), defaultTo, output)
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
        
        Public Async Function AssignPrefixAsync(input As String) As Task
            DiscordConfig.Prefix = input
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
