using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Railgun.Core.Extensions
{
    public static class TextChannelExtensions
    {
        public static Task SendStringAsFileAsync(this ITextChannel tc, string filename, string output, string msgText = null, bool includeGuildName = true)
		{
			var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(output));
			var outputFilename = (includeGuildName ? $"{tc.Guild.Name}-" : "") + filename;
			return tc.SendFileAsync(outputStream, outputFilename, msgText);
		}
    }
}