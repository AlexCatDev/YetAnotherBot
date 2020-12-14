using Discord.WebSocket;
using System.Linq;

namespace BotPluginBase
{
    public static class Extensions
    {
        public static int Clamp(this int value, int min, int max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;

            return value;
        }

        /// <summary>
        /// I fucking love linq and it's unreadable tricks, this is very bad for performance but who cares
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static SocketGuild GetGuild(this DiscordSocketClient client, SocketMessage msg) => client.Guilds.Where((x) => x.Channels.Any(y => y.Id == msg.Channel.Id)).First();
    }
}
