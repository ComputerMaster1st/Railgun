Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Api
Imports RailgunVB.Core.Preconditions

Namespace Commands.Fun
    
    <Group("cat")>
    Public Class Cat
        Inherits ModuleBase
    
        Private ReadOnly _randomCat As RandomCat

        Public Sub New(randomCat As RandomCat)
            _randomCat = randomCat
        End Sub
        
        <Command, BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function CatAsync() As Task
            await Context.Channel.SendFileAsync(await _randomCat.GetRandomCatAsync(), "CatImg.png")
        End Function
        
    End Class
    
End NameSpace