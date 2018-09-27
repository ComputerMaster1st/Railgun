Imports Discord.Commands
Imports RailgunVB.Core.Api

Namespace Commands
    
    <Group("cat")>
    Public Class Cat
        Inherits ModuleBase
    
        Private ReadOnly _randomCat As RandomCat

        Public Sub New(randomCat As RandomCat)
            _randomCat = randomCat
        End Sub
        
        <Command>
        Public Async Function CatAsync() As Task
            await Context.Channel.SendFileAsync(await _randomCat.GetRandomCatAsync(), "CatImg.png")
        End Function
        
    End Class
    
End NameSpace