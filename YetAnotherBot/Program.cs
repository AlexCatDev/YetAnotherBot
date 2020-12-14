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
using System.Xml.Linq;
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

            pluginResolver.ResolvePlugins("./Plugins/", null);

            DiscordSocketClient client = new DiscordSocketClient();

            bool signalKill = false;

            CommandHandler.AddCommand(">info", "Shows bot info", (msg, sMsg) =>
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
                desc += $"Shard ID: **{client.ShardId}**\n";
                for (int i = 0; i < loadedPlugins.Count; i++)
                {
                    desc += $"Plugin {i+1}: **{loadedPlugins[i].PluginName}** : *{loadedPlugins[i].PluginDescription}*\n";
                }

                embed.WithDescription(desc);
                embed.WithColor(Color.Blue);
                embed.WithFooter($"Loaded plugins: {loadedPlugins.Count} Active commands: {CommandHandler.ActiveCommands}");
                sMsg.Channel.SendMessageAsync("", false, embed.Build());
            });

            CommandHandler.AddCommand(">reload", "Reloads plugins", (msg, sMsg) =>
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

            CommandHandler.AddCommand(">help", "Shows this embed", (msg, sMsg) =>
            {
                Pages commandPages = new Pages();

                string desc = "";

                EmbedBuilder eb = new EmbedBuilder();
                eb.WithAuthor("Commands available");

                for (int i = 0; i < CommandHandler.Commands.Count; i++)
                {
                    string tempDesc = "";
                    var currentCommand = CommandHandler.Commands[i];

                    tempDesc += $"**{i+1}.** {currentCommand.Key} **{currentCommand.Value.Description}**\n";

                    if (desc.Length + tempDesc.Length < 2048)
                        desc += tempDesc;
                    else
                    {
                        eb.WithDescription(desc);
                        commandPages.AddContent(eb);

                        eb = new EmbedBuilder();
                        eb.WithAuthor("Commands available");

                        desc = "";
                        desc += tempDesc;
                    }
                }

                eb.WithDescription(desc);
                commandPages.AddContent(eb);


                //eb.WithDescription(desc);
                //eb.WithColor(Color.Green);
                PagesHandler.SendPages(sMsg.Channel, commandPages);
            });

            CommandHandler.AddCommand(">yep", "test123", (msg, sMsg) =>
            {
                Pages p = new Pages();

                EmbedBuilder a = new EmbedBuilder();
                a.WithAuthor("Fun facts about something");
                a.WithDescription("nice");

                EmbedBuilder b = new EmbedBuilder();
                b.WithAuthor("Fun facts about something");
                b.WithDescription("ok cool");

                EmbedBuilder c = new EmbedBuilder();
                c.WithAuthor("Fun facts about something");
                c.WithDescription("very cool");

                EmbedBuilder d = new EmbedBuilder();
                d.WithAuthor("Fun facts about something");
                d.WithDescription("funny haha");

                p.AddContent(a);
                p.AddContent(b);
                p.AddContent(c);
                p.AddContent(d);

                PagesHandler.SendPages(sMsg.Channel, p);
            });

            client.UserJoined += (s) =>
            {
                if (s.Id == 165848428360892416)
                    s.BanAsync(0, "Retard");


                return Task.Delay(0);
            };

            client.MessageReceived += (s) =>
            {
                if(s.Content.ToLower() == "::kys" && s.Author.Id == 591339926017146910)
                {
                    s.Channel.SendMessageAsync("Okay kammerat :ok_hand:").GetAwaiter().GetResult();
                    signalKill = true;
                    return Task.Delay(0);
                }

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
                return Task.Delay(0);
            };

            client.ReactionAdded += (s, e, x) =>
            {
                if (x.User.Value.IsBot)
                    return Task.Delay(0);

                var msg = s.GetOrDownloadAsync().Result;

                PagesHandler.Handle(msg, x);

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
