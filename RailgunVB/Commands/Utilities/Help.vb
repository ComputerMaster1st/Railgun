Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Preconditions

Namespace Commands.Utilities
    
    <Group("help")>
    Public Class Help
        Inherits ModuleBase
        
        <Command>
        Public Function SendHelpAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendFormat("For the list of commands, please visit our wiki @ <{0}>", 
                    Format.Bold("https://github.com/ComputerMaster1st/RailgunVB/wiki")).AppendLine() _
                .AppendFormat("If you have any questions or need help, contact the developer via the developer's Discord: {0}", 
                    Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine() _
                .AppendLine() _ 
                .AppendFormat("Would you like to support Railgun even further? {0}", 
                                            Format.Bold("<https://www.buymeacoffee.com/computermaster1>"))
            
            Return ReplyAsync(output.ToString())
        End Function
        
    End Class
    
End NameSpace