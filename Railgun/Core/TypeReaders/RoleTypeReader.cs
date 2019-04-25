using System;
using Discord;
using Railgun.Core.Parsers;

namespace Railgun.Core.TypeReaders
{
	public class RoleTypeReader : DiscordTypeReader
	{
		public override Type SupportedType => typeof(IRole);

		public override bool TryParse(string value, SystemContext context, out object result)
		{
			if (MentionParser.TryParseRole(value, out ulong id))
			{
				result = context.Guild.GetRole(id);
				return true;
			}

			result = default;
			return false;
		}
	}
}