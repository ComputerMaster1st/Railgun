Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Preconditions
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Fun

Namespace Commands.Fun
    
    <Group("bite")>
    Public Class Bite
        Inherits SystemBase
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _commandUtils As CommandUtils

        Public Sub New(config As MasterConfig, commandUtils As CommandUtils)
            _config = config
            _commandUtils = commandUtils
        End Sub
        
        <Command>
        Public Async Function BiteAsync(Optional user As IUser = Nothing) As Task
            Dim data As FunBite = Await Context.Database.FunBites.GetAsync(Context.Guild.Id)
            
            If Data Is Nothing OrElse data.Bites.Count < 1
                Dim output As New StringBuilder
                
                output.AppendLine("There are no bite sentences available. Please ask an admin to set some up.") _
                    .AppendFormat("Admin, to add a sentence, use {0}. You will need to place `{1} & {2} in the sentence somewhere to make this work.", Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>"), Format.Code("<biter>"), 
                                  Format.Code("<bitee>"))
                
                await ReplyAsync(output.ToString())
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"Bite is currently {Format.Bold("disabled")} on this server.")
                Return
            End If
            
            Dim rand As New Random
            Dim i
            Dim biter As IGuildUser = Nothing
            Dim bitee As IGuildUser = Nothing
            
            If user Is Nothing
                i = rand.Next(1,2)
            Else 
                i = rand.Next(3,6)
            End If
            
            Select i
                Case 1
                    biter = await Context.Guild.GetCurrentUserAsync()
                    bitee = await Context.Guild.GetUserAsync(Context.User.Id)
                    Exit Select
                Case 2
                    biter = await Context.Guild.GetUserAsync(Context.User.Id)
                    bitee = await Context.Guild.GetCurrentUserAsync()
                    Exit Select
                Case 3
                    biter = await Context.Guild.GetUserAsync(user.Id)
                    bitee = await Context.Guild.GetCurrentUserAsync()
                    Exit Select
                Case 4
                    biter = await Context.Guild.GetCurrentUserAsync()
                    bitee = await Context.Guild.GetUserAsync(user.Id)
                    Exit Select
                Case 5
                    biter = await Context.Guild.GetUserAsync(user.Id)
                    bitee = await Context.Guild.GetUserAsync(Context.User.Id)
                    Exit Select
                Case 6
                    biter = await Context.Guild.GetUserAsync(Context.User.Id)
                    bitee = await Context.Guild.GetUserAsync(user.Id)
                    Exit Select
            End Select
            
            Dim biterName As String = await _commandUtils.GetUsernameOrMentionAsync(biter)
            Dim biteeName As String = await _commandUtils.GetUsernameOrMentionAsync(bitee)
            
            Dim biteMessage As String = data.GetBite().Replace("<biter>", Format.Bold(biterName)) _ 
                    .Replace("<bitee>", Format.Bold(biteeName))
            
            await ReplyAsync(biteMessage)
        End Function
        
        <Command("add")>
        Public Async Function AddAsync(<Remainder> msg As String) As Task
            If String.IsNullOrWhiteSpace(msg)
                await ReplyAsync("You didn't specify a sentence!")
                Return
            End If
            
            Dim data As FunBite = Await Context.Database.FunBites.GetOrCreateAsync(Context.Guild.Id)
                
            If Not (data.IsEnabled)
                await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.")
                Return
            End If
            
            data.AddBite(msg)
            
            await ReplyAsync($"Added Sentence: {Format.Code(msg)}")
        End Function
        
        <Command("list"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function ListAsync() As Task
            Dim data As FunBite = Await Context.Database.FunBites.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync($"There are no bite sentences available. Use {Format.Code(
                    $"{_config.DiscordConfig.Prefix}bite add <message>")} to add some.")
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.")
                Return
            ElseIf data.Bites.Count < 1
                await ReplyAsync($"There are no bite sentences available. To add a sentence, use {Format.Code(
                    $"{_config.DiscordConfig.Prefix}bite add <sentence>")}. You will need to place { _ 
                        Format.Code("<biter>")} & {Format.Code("<bitee>")} in the sentence somewhere to make this work.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendFormat("List of Bite Sentences: ({0} Total)", data.Bites.Count).AppendLine().AppendLine()
            
            For Each bite As String In data.Bites
                output.AppendFormat("[{0}] {1}", Format.Code(data.Bites.IndexOf(bite).ToString()), bite).AppendLine()
            Next
            
            If output.Length < 1900 
                await ReplyAsync(output.ToString())
                Return
            End If
            
            Await SendStringAsFileAsync(Context.Channel, "Bites.txt", output.ToString(), 
                $"List of {Format.Bold(data.Bites.Count.ToString())} Bite Sentences")
        End Function
        
        <Command("remove"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function RemoveAsync(index As Integer) As Task
            Dim data As FunBite = Await Context.Database.FunBites.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.Bites.Count < 1
                await ReplyAsync("The list of bite sentences is already empty.")
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.")
                Return
            ElseIf index < 0 OrElse index >= data.Bites.Count
                await ReplyAsync("The Message Id provided is out of bounds. Please recheck via Bite List.")
                Return
            End If

            data.Bites.RemoveAt(index)
            
            await ReplyAsync("Message Removed.")
        End Function
        
        <Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function AllowDenyAsync() As Task
            Dim data As FunBite = Await Context.Database.FunBites.GetOrCreateAsync(Context.Guild.Id)
            
            data.IsEnabled = Not (data.IsEnabled)
            
            await ReplyAsync($"Bites are now {If(data.IsEnabled, Format.Bold("enabled"), Format.Bold("disabled"))}!")
        End Function
        
        <Command("reset"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function ResetAsync() As Task
            Dim data As FunBite = Await Context.Database.FunBites.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("Bites has no data to reset.")
                Return
            End If
            
            Context.Database.FunBites.Remove(data)
            
            await ReplyAsync("Bites has been reset.")
        End Function
        
    End Class
    
End NameSpace