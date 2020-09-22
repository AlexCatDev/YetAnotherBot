using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using BotPluginBase;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace YetAnotherBot
{
    class Program
    {
        private static PluginResolver<IBotPlugin> pluginResolver = new PluginResolver<IBotPlugin>();

        private static List<IBotPlugin> loadedPlugins = new List<IBotPlugin>();

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Directory.CreateDirectory("./Plugins");

            pluginResolver.PluginResolved += (s, e) =>
            {
                loadedPlugins.Add(e.Plugin);
                Logger.Log($"Successfully loaded {e.Plugin.PluginName}", LogLevel.Success);
            };

            pluginResolver.PluginResolveError += (s, e) =>
            {
                Logger.Log($"Error solving plugin: {e.Exception.Message} STACKTRACE: {e.Exception.StackTrace}", LogLevel.Error);
            };

            DiscordSocketClient client = new DiscordSocketClient();

            bool signalKill = false;

            CommandHandler.AddCommand(">info", (msg, sMsg) =>
            {
                var runtimeVer = RuntimeInformation.FrameworkDescription;

                EmbedBuilder embed = new EmbedBuilder();

                embed.WithTitle("Bot Runtime Info");
                string desc = "";


                desc += "[Github Link](https://github.com/CSharpProgramming/YetAnotherBot)\n";

                desc += $"Runtime: **{runtimeVer}**\n";
                desc += $"OS: **{RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}**\n";
                desc += $"CPU Cores: **{Environment.ProcessorCount}**\n";
                desc += "Oppai Version: **3.3.0-b237**\n";
                desc += $"Ping: **{client.Latency} MS**\n";
                for (int i = 0; i < loadedPlugins.Count; i++)
                {
                    desc += $"Plugin {i+1}: **{loadedPlugins[i].PluginName}** : *{loadedPlugins[i].PluginDescription}*\n";
                }

                embed.WithDescription(desc);
                embed.WithColor(Color.Blue);
                embed.WithFooter("Loaded plugins: " + loadedPlugins.Count + "");
                sMsg.Channel.SendMessageAsync("", false, embed.Build());
            });

            CommandHandler.AddCommand(">reload", (msg, sMsg) =>
            {
                sMsg.Channel.SendMessageAsync($"Reloading plugins...");
                for (int i = 0; i < loadedPlugins.Count; i++)
                {
                    loadedPlugins[i].Unloading();
                }
                loadedPlugins.Clear();
                pluginResolver.ResolvePlugins("./Plugins/", null);
                sMsg.Channel.SendMessageAsync($"Done, loaded: {loadedPlugins.Count} plugins");
            });

            client.MessageReceived += (s) =>
            {
                if (s.Author.IsBot)
                    return Task.Delay(0);

                CommandHandler.Handle(s);

                for (int i = 0; i < loadedPlugins.Count; i++)
                {
                    try
                    {

                        if (loadedPlugins[i].HandleMessage(s, client))
                            break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error invoking plugin: " + i, LogLevel.Warning);
                        Logger.Log(ex.StackTrace, LogLevel.Error);
                    }
                }

                Logger.Log(s.Channel.Name + "->" + s.Author.Username + ": " + s.Content);
                return Task.Delay(0);
            };

            client.Ready += () =>
            {
                Logger.Log("Hello logged in as " + client.CurrentUser.Username + " ready to serve!", LogLevel.Success);
                pluginResolver.ResolvePlugins("./Plugins/", null);
                return Task.Delay(0);
            };

            client.LoginAsync(TokenType.Bot, Credentials.DISCORD_API_KEY);
            client.StartAsync();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                Thread.Sleep(10);
                if (sw.ElapsedMilliseconds >= 5000 || signalKill)
                {
                    sw.Restart();
                    if (signalKill)
                    {
                        client.LogoutAsync().GetAwaiter().GetResult();
                        Logger.Log("logged out bye, press enter to continue", LogLevel.Warning);
                        Console.ReadLine();
                        break;
                    }
                }
            }
        }
    }
}
