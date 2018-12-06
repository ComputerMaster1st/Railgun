using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Finite.Commands;
using Railgun.Core.Commands.Attributes;

namespace Railgun.Core.Commands
{
    
    public class SystemCommandParser<TContext> : DefaultCommandParser<TContext> where TContext : class, ICommandContext
    {
        private enum TokenizerState {
            Normal,
            EscapeCharacter,
            ParameterSeparator,
            QuotedString
        }

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

        protected override TokenizerResult Tokenize(string commandText, int prefixLength) {
            TokenizerResult Failure(TokenizerFailureReason reason, int position)
                => new TokenizerResult((int)reason, commandText, position);

            if (prefixLength >= commandText.Length) throw new ArgumentOutOfRangeException(nameof(prefixLength));

            var paramBuilder = new StringBuilder();
            var result = new List<string>();
            var state = TokenizerState.Normal;
            // var beginQuote = default(char);

            for (int i = prefixLength; i < commandText.Length; i++) {
                char c = commandText[i];
                var isLastCharacter = i == commandText.Length - 1;

                switch (state) {
                    case TokenizerState.Normal
                        when char.IsWhiteSpace(c):
                        result.Add(paramBuilder.ToString());
                        state = TokenizerState.ParameterSeparator;
                        break;
                    case TokenizerState.Normal
                        when IsEscapeCharacter(c) && isLastCharacter:
                        return Failure(
                            TokenizerFailureReason.UnfinishedEscapeSequence,
                            i);
                    // case TokenizerState.Normal
                    //     when IsQuoteCharacter(c):
                    //     return Failure(TokenizerFailureReason.UnexpectedQuote, i);
                    case TokenizerState.Normal
                        when IsEscapeCharacter(c):
                        state = TokenizerState.EscapeCharacter;
                        break;

                    case TokenizerState.EscapeCharacter
                        when IsEscapableCharacter(c):
                        state = TokenizerState.Normal;
                        goto default;
                    case TokenizerState.EscapeCharacter:
                        return Failure(
                            TokenizerFailureReason.InvalidEscapeSequence, i);

                    // case TokenizerState.ParameterSeparator
                    //     when IsQuoteCharacter(c) && isLastCharacter:
                    //     return Failure(TokenizerFailureReason.UnfinishedQuotedString, i);
                    // case TokenizerState.ParameterSeparator
                    //     when IsQuoteCharacter(c):
                    //     state = TokenizerState.QuotedString;
                    //     beginQuote = c;
                    //     paramBuilder.Clear();
                    //     break;
                    case TokenizerState.ParameterSeparator
                        when !char.IsWhiteSpace(c):
                        state = TokenizerState.Normal;
                        paramBuilder.Clear();
                        goto default;

                    // case TokenizerState.QuotedString
                    //     when IsCompletedQuote(beginQuote, c):
                    //     state = TokenizerState.Normal;
                    //     break;
                    // case TokenizerState.QuotedString
                    //     when isLastCharacter:
                    //     return Failure(TokenizerFailureReason.UnfinishedQuotedString, i);

                    default:
                        paramBuilder.Append(c);
                        break;
                }
            }

            // Add any final parameters
            result.Add(paramBuilder.ToString());

            if (state != TokenizerState.Normal)
                return Failure(TokenizerFailureReason.InvalidState,
                    commandText.Length);

            return new TokenizerResult(result.ToArray());
        }
    }
}