Imports System.IO
Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Fun

Namespace Commands.Fun
    
    <Group("rst")>
    Public Class Rst
        Inherits SystemBase
    
        Private ReadOnly _config As MasterConfig

        Public Sub New(config As MasterConfig)
            _config = config
        End Sub
        
        <Command>
        Public Async Function RstAsync() As Task
            Dim data As FunRst = Await Context.Database.FunRsts.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code(
                    $"{_config.DiscordConfig.Prefix}rst add [message]")}.")
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.")
                Return
            End If
            
            Dim msg As String = data.GetRst()
            
            If String.IsNullOrEmpty(msg) Then msg = $"RST is empty! Please add some stuff using {Format.Code(
                $"{_config.DiscordConfig.Prefix}rst add [message]")}."
            
            await ReplyAsync(msg)
        End Function
        
        <Command("add")>
        Public Async Function AddAsync(<Remainder> msg As String) As Task
            If String.IsNullOrWhiteSpace(msg)
                Await ReplyAsync("Your message was empty. Please add a message to add.")
                Return
            End If
            
            Dim data As FunRst = Await Context.Database.FunRsts.GetOrCreateAsync(Context.Guild.Id)
            
            If Not (data.IsEnabled)
                Await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.")
                Return
            End If
            
            data.AddRst(msg)
            
            Await ReplyAsync($"Added To RST: {Format.Code(msg)}")
        End Function
        
        <Command("remove"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function RemoveAsync(index As Integer) As Task
            Dim data As FunRst = Await Context.Database.FunRsts.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code(
                    $"{_config.DiscordConfig.Prefix}rst add [message]")}.")
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.")
                Return
            ElseIf index < 0 OrElse index >= data.Rst.Count
                await ReplyAsync("The Message Id provided is out of bounds. Please recheck via RST List.")
                Return
            End If
            
            data.Rst.RemoveAt(index)
            
            await ReplyAsync("Message Removed!")
        End Function
        
        <Command("list"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function ListAsync() As Task
            Dim data As FunRst = Await Context.Database.FunRsts.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code(
                    $"{_config.DiscordConfig.Prefix}rst add [message]")}.")
                Return
            ElseIf Not (data.IsEnabled)
                await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendLine("Randomly Selected Text List :").AppendLine()
            
            For Each msg As String In data.Rst
                output.AppendFormat("[{0}] {1}", Format.Code(data.Rst.IndexOf(msg).ToString()), msg).AppendLine()
            Next
            
            If output.Length < 1950
                await ReplyAsync(output.ToString())
                Return
            End If
            
            Dim filename As String = $"{Context.Guild.Id}_RST.txt"
            
            Await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename)
            
            File.Delete(filename)
        End Function
        
        <Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function AllowDenyAsync() As Task
            Dim data As FunRst = Await Context.Database.FunRsts.GetOrCreateAsync(Context.Guild.Id)
            
            data.IsEnabled = Not (data.IsEnabled)
            
            await ReplyAsync($"RST is now {If(data.IsEnabled, Format.Bold("enabled"), Format.Bold("disabled"))}!")
        End Function
        
        <Command("reset"), UserPerms(GuildPermission.ManageMessages)>
        Public Async Function ResetAsync() As Task
            Dim data As FunRst = Await Context.Database.FunRsts.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("RST has no data to reset.")
                Return
            End If
            
            Context.Database.FunRsts.Remove(data)
            
            await ReplyAsync("RST has been reset.")
        End Function
        
    End Class
    
End NameSpace