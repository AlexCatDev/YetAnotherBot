using Discord;
using Discord.WebSocket;
using System;

namespace BotPluginBase
{
    public interface IBotPlugin
    {
        string PluginName { get; }
        string PluginDescription { get; }

        bool HandleMessage(SocketMessage msg, DiscordSocketClient client);

        void Unloading();
    }
}
