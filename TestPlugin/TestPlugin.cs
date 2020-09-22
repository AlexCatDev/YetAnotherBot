using BotPluginBase;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Net.Mail;

namespace TestPlugin
{
    public class TestPlugin : IBotPlugin
    {
        public string PluginName => "Test Plugin";

        public string PluginDescription => "I reply annoyingly to your messages";

        public TestPlugin()
        {

        }

        public bool HandleMessage(SocketMessage msg, DiscordSocketClient client)
        {
            if(msg.MentionedUsers.Any(a=>a.Id == 594840169115418624)) {
                msg.Channel.SendMessageAsync("No u");
                return false;
            }

            string[] args = msg.Content.Split(' ');

            if(args[0].ToLower() == "hi")
            {
                msg.Channel.SendMessageAsync("Hi!");
                return true;
            }

            if (args[0].ToLower() == "test")
            {
                msg.Channel.SendMessageAsync("Hi!");
                return true;
            }

            if (args[0].StartsWith("::"))
            {
                switch(args[0].Remove(0, 2).ToLower())
                {
                    case "hi":
                        msg.Channel.SendMessageAsync("hi");
                        return true;
                    case "funny":
                        msg.Channel.SendMessageAsync(@"https://www.youtube.com/watch?v=MkPjWT5d-J0");
                        break;
                }
            }

            return false;
        }

        public void Unloading()
        {

        }
    }
}
