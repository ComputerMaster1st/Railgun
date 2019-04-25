using System.Globalization;

namespace Railgun.Core.Parsers
{
	public static class MentionParser
	{
		public static bool TryParseUser(string text, out ulong userId)
		{
			if (text.Length >= 3 && text[0] == '<' && text[1] == '@' && text[text.Length - 1] == '>') 
			{
				if (text.Length >= 4 && text[2] == '!') text = text.Substring(3, text.Length - 4); //<@!123>
				else text = text.Substring(2, text.Length - 3); //<@123>

				if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out userId)) return true;
			}

			userId = 0;
			return false;
		}

		public static bool TryParseChannel(string text, out ulong channelId)
		{
			if (text.Length >= 3 && text[0] == '<' && text[1] == '#' && text[text.Length - 1] == '>')
			{
				text = text.Substring(2, text.Length - 3); //<#123>

				if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out channelId)) return true;
			}

			channelId = 0;
			return false;
		}

		public static bool TryParseRole(string text, out ulong roleId)
		{
			if (text.Length >= 4 && text[0] == '<' && text[1] == '@' && text[2] == '&' && text[text.Length - 1] == '>')
			{
				text = text.Substring(3, text.Length - 4); //<@&123>

				if (ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out roleId)) return true;
			}

			roleId = 0;
			return false;
		}
	}
}