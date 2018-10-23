Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core

Namespace Commands.Utilities
    
    <Group("donate")>
    Public Class Donate
        Inherits SystemBase
        
        <Command>
        Public Function SendCoffeeAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendFormat("Would you like to support Railgun even further? {0}", 
                                Format.Bold("<https://www.buymeacoffee.com/computermaster1>")).AppendLine() _
                .AppendLine() _
                .AppendFormat("{0}", Format.Bold(Format.Underline("Why should you donate?"))).AppendLine() _
                .AppendLine("We provide our services free of charge, even if other bots prefer to put certain features/functions behind a pay wall. Well, we do not wish to do that. Nor do we want to reduce the quality of anything. All of the donations given will be used for development & to cover server costs.") _
                .AppendLine() _
                .AppendLine("We thank you for supporting Railgun and her services along with her development!")

            Return ReplyAsync(output.ToString())
        End Function
        
    End Class
    
End NameSpace