Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Preconditions

Namespace Commands.Utilities
    
    <Group("help"), BotPerms(ChannelPermission.AttachFiles)>
    Public Class Help
        Inherits ModuleBase
        
        <Command>
        Public Function SendHelpAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendLine("Please refer to the PDF file attached with this message. It shall contain everything you need to know on how to use Railgun.") _
                 .AppendFormat("If you have any questions or need help, contact the developer via the developer's Discord: {0}", 
                               Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine() _
                 .AppendLine().AppendFormat("Would you like to support Railgun even further? {0}", 
                                            Format.Bold("<https://www.buymeacoffee.com/computermaster1>"))
            
            Return Context.Channel.SendFileAsync("Railgun.pdf", output.ToString())
        End Function
        
    End Class
    
End NameSpace