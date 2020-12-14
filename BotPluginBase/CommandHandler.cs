using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BotPluginBase
{

    public static class CommandHandler
    {
        public class Command
        {
            public string Description;
            public CommandCallBack CommandCallBack;

            public bool IsActive => CommandCallBack != null;
        }

        public delegate void CommandCallBack(string[] args, SocketMessage socketMsg);

        private static Dictionary<string, Command> commands = new Dictionary<string, Command>();

        public static int ActiveCommands => commands.Count;

        public static IReadOnlyList<KeyValuePair<string, Command>> Commands => commands.ToList();

        public static void AddCommand(string trigger, string description, CommandCallBack cmdCallback)
        {
            var command = new Command() { Description = description, CommandCallBack = cmdCallback };

            if (commands.ContainsKey(trigger))
                commands[trigger] = command;
            else
                commands.Add(trigger, command);
        }

        public static void Handle(SocketMessage socketMsg)
        {
            string[] args = socketMsg.Content.ToLower().Split(' ');

            Command cmd;
            if (commands.TryGetValue(args[0].ToLower(), out cmd))
            {
                cmd.CommandCallBack?.Invoke(args, socketMsg);
            }
        }
    }
}
