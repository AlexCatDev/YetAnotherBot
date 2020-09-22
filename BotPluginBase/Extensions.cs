using Discord.WebSocket;
using System.Linq;

namespace BotPluginBase
{
    public static class Extensions
    {
        /// <summary>
        /// I fucking love linq and it's unreadable tricks, this is very bad for performance but who cares
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static SocketGuild GetGuild(this DiscordSocketClient client, SocketMessage msg) => client.Guilds.Where((x) => x.Channels.Any(y => y.Id == msg.Channel.Id)).First();
    }
}
