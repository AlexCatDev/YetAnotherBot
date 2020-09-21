using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace BotPluginBase
{
	public static class CommandHandler
	{
		internal class Command
		{
			public Action<string, SocketMessage> OnCommand { get; set; }
		}

		private static Dictionary<string, Command> commands = new Dictionary<string, Command>();

		public static void Handle(SocketMessage msg)
		{
			string content = msg.Content.ToLower();
			string commandString = content.Contains(" ") ? content.Substring(0, content.IndexOf(" ")) : content;

			if (commands.TryGetValue(commandString, out Command outCommand))
			{
				outCommand.OnCommand?.Invoke(msg.Content.Remove(0, commandString.Length), msg);
			}
		}

		public static void AddCommand(string commandString, Action<string, SocketMessage> onMessage)
		{
			commands.Add(commandString, new Command() { OnCommand = onMessage });
		}

		public static void RemoveCommand(string commandString)
		{
			commands.Remove(commandString);
		}
	}
}