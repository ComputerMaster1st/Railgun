using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;

namespace Railgun.Core.Extensions
{
    public static class TextChannelExtensions
    {
        private const int Retry = 3; 

        public static async Task<IUserMessage> TrySendMessageAsync(this ITextChannel tc, string msg = null, bool isTTS = false, Embed embed = null) {
            Exception normalException = null;

            for (var i=0; i<Retry; i++) {
                try {
                    return await tc.SendMessageAsync(msg, isTTS, embed);
                } catch (HttpRequestException e) {
                    normalException = e;
                    continue;
                } catch (Exception e) {
                    normalException = e;
                    break;
                }
            }

            throw normalException;
        }

        public static async Task<IUserMessage> TrySendMessageAsync(this IDMChannel tc, string msg = null, bool isTTS = false, Embed embed = null) {
            Exception normalException = null;

            for (var i=0; i<Retry; i++) {
                try {
                    return await tc.SendMessageAsync(msg, isTTS, embed);
                } catch (HttpRequestException) {
                    continue;
                } catch (Exception e) {
                    normalException = e;
                    break;
                }
            }

            throw normalException;
        }
    }
}