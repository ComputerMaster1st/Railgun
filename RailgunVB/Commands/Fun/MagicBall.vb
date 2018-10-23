Imports Discord.Commands
Imports RailgunVB.Core

Namespace Commands.Fun
    
    <Group("8ball")>
    Public Class MagicBall
        Inherits SystemBase
        
        Private ReadOnly _responses As String() = {
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            "Reply hazy try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful."
        }
    
        <Command>
        Public Function MagicBallAsync(<Remainder> Optional query As String = Nothing) As Task
            Dim rand As New Random
            Dim index As Integer = rand.Next(0, (_responses.Length - 1))
            return ReplyAsync($"{If(string.IsNullOrWhiteSpace(query), "", 
                                    $"Your Question: {query} || ")}8Ball's Response: {_responses(index)}")
        End Function
        
    End Class
    
End NameSpace