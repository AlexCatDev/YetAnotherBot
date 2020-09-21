using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotPluginBase
{
    public static class CommandHandler
    {
        internal class Command
        {
            public string CommandString { get; set; }
            public Action<string, SocketMessage> OnCommand { get; set; }
        }

        private static List<Command> commands = new List<Command>();

        public static void Handle(SocketMessage msg)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (msg.Content.ToLower().StartsWith(commands[i].CommandString))
                {
                    commands[i].OnCommand?.Invoke(msg.Content.Remove(0, commands[i].CommandString.Length), msg);
                    break;
                }
            }
        }

        public static void AddCommand(string commandString, Action<string, SocketMessage> onMessage)
        {
            commands.Add(new Command() { CommandString = commandString, OnCommand = onMessage });
        }

        public static void RemoveCommand(string commandString)
        {
            commands.RemoveAll(a=>a.CommandString == commandString);
        }
    }
}
