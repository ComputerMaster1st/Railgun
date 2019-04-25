using System.IO;

namespace Railgun.Core
{
    public static class Directories
    {
        public static string ConsoleLog { get; } = "log_console";
        public static string BotLog { get; } = "log_bot";

        public static void CheckDirectories()
        {
            if (!Directory.Exists(ConsoleLog)) Directory.CreateDirectory(ConsoleLog);
            if (!Directory.Exists(BotLog)) Directory.CreateDirectory(BotLog);
        }
    }
}