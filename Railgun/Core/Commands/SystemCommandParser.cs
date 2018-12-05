using System;
using System.Linq;
using System.Text;
using Finite.Commands;
using Railgun.Core.Commands.Attributes;

namespace Railgun.Core.Commands
{
    public class SystemCommandParser<TContext> : DefaultCommandParser<TContext> where TContext : class, ICommandContext
    {
        protected override bool GetArgumentsForMatch(ICommandService commands, CommandMatch match, out object[] result) {
            bool TryParseMultiple(ParameterInfo argument, int startPos, out object[] parsed) {
                parsed = new object[match.Arguments.Length - startPos];

                for (int i = startPos; i < match.Arguments.Length; i++) {
                    var ok = TryParseObject(commands, argument, match.Arguments[i], out var value);

                    if (!ok) return false;

                    parsed[i - startPos] = value;
                }

                return true;
            }

            // ============================================================================================================

            var parameters = match.Command.Parameters;
            result = new object[parameters.Count];

            for (int i = 0; i < parameters.Count; i++) {
                var argument = parameters[i];

                if ((i == parameters.Count - 1) && argument.Attributes.Any(x => x is ParamArrayAttribute)) {
                    if (!TryParseMultiple(argument, i, out var multiple)) return false;
    
                    result[i] = multiple;
                } else if (argument.Attributes.Any(x => x is Remainder)) {
                    var output = new StringBuilder();

                    for (int subIndex = i; subIndex < match.Arguments.Length; subIndex++)
                        output.AppendFormat("{0} ", match.Arguments[subIndex]);
                    
                    var ok = TryParseObject(commands, argument, output.ToString().TrimEnd(' '), out var value);

                    if (!ok) return false;

                    result[i] = value;
                } else {
                    var ok = TryParseObject(commands, argument, match.Arguments[i], out var value);

                    if (!ok) return false;

                    result[i] = value;
                }
            }
            return true;
        }
    }
}