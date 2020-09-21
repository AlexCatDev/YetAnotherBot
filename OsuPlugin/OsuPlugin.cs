using BotPluginBase;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace OsuPlugin
{
    public class OsuPlugin : IBotPlugin
    {
        public string PluginName => "osu! Plugin";

        public string PluginDescription => "I do osu! stuff";

        //Ulong Discord user id, string osu name
        private Dictionary<ulong, string> discordUserToOsuUser = new Dictionary<ulong, string>();

        private Dictionary<ulong, ulong> discordChannelToBeatmap = new Dictionary<ulong, ulong>();

        private Dictionary<ulong, List<OsuAPI.BestPlayResult>> discordChannelToBeatmapTop = new Dictionary<ulong, List<OsuAPI.BestPlayResult>>();

        /// <summary>
        /// TODO: Format PP and Acc with {0:.00}, always show two decimal places..
        /// </summary>

        public OsuPlugin()
        {
            OsuAPI oApi = new OsuAPI(Credentials.OSU_API_KEY);

            if (File.Exists("./OsuUsers"))
            {
                discordUserToOsuUser = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText("./osuUsers"));

                Console.WriteLine("Loaded: " + discordUserToOsuUser.Count + " users");
            }

            //Optimized
            CommandHandler.AddCommand(">rs", (msg, sMsg) =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                if (sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You haven't set a profile, do >set <your_osu_name> to set your osu user");
                    return;
                }

                bool showList = false;

                string userToCheck = "";

                string[] args = sMsg.Content.Split(' ');
                if (args.Length > 2)
                {
                    userToCheck = args[1];

                    if (args[2].ToLower() == "-l")
                        showList = true;

                }
                else if (args.Length > 1)
                {
                    if (args[1].ToLower() == "-l")
                        showList = true;
                    else
                        userToCheck = args[1];
                }

                if(userToCheck == "")
                    userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                EmbedBuilder embedBuilder = new EmbedBuilder();
                string description = "";
                Logger.Log($"getting recent plays for '{userToCheck}'", LogLevel.Info);

                try
                {
                    List<OsuAPI.RecentPlayResult> recentUserPlays = oApi.GetRecentPlays(userToCheck, 20);

                    if (recentUserPlays.Count == 0)
                    {
                        sMsg.Channel.SendMessageAsync($"**{userToCheck} don't have any recent plays** <:sadChamp:593405356864962560>");
                        return;
                    }

                    if (discordChannelToBeatmap.ContainsKey(sMsg.Channel.Id))
                        discordChannelToBeatmap[sMsg.Channel.Id] = recentUserPlays[0].BeatmapID;
                    else
                        discordChannelToBeatmap.Add(sMsg.Channel.Id, recentUserPlays[0].BeatmapID);

                    int count = 0;
                    int beatmapSetID = 0;

                    foreach (var rup in recentUserPlays)
                    {
                        count++;

                        string map = Utils.DownloadBeatmap(rup.BeatmapID.ToString());

                        if (beatmapSetID == 0)
                            beatmapSetID = Utils.FindBeatmapSetID(map);

                        EZPPResult result = EZPP.Calculate(map, rup.MaxCombo, rup.Count100,
                        rup.Count50, rup.CountMiss, rup.EnabledMods);

                        float pp = MathF.Round(result.PP, 2);
                        float stars = MathF.Round(result.StarRating, 2);
                        float acc = MathF.Round(result.Accuracy, 2);

                        float objectsEncountered = rup.Count300 + rup.Count100 + rup.Count50 + rup.CountMiss;
                        float percentage100 = ((float)rup.Count100 + rup.CountMiss) / objectsEncountered;
                        float percentage50 = rup.Count50 / objectsEncountered;
                        int expected100 = (int)(result.TotalHitObjects * percentage100);
                        int expected50 = (int)(result.TotalHitObjects * percentage50);


                        EZPPResult resultFC = EZPP.Calculate(map, result.MaxCombo, expected100, expected50, 0, rup.EnabledMods);

                        float ppFC = MathF.Round(resultFC.PP, 2);
                        float accFC = MathF.Round(resultFC.Accuracy, 2);

                        string temp = "";

                        temp += $"**{count}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(rup.BeatmapID.ToString())}) **+{rup.EnabledMods.ToFriendlyString()}** [{stars}★]\n";
                        temp += $"▸ {Utils.GetEmoteForRankLetter(rup.RankLetter)} ▸ **{pp}PP** ({ppFC}PP for {accFC}% FC) ▸ {acc}%\n";
                        temp += $"▸ {rup.Score} ▸ x{rup.MaxCombo}/{result.MaxCombo} ▸ [{rup.Count300}/{rup.Count100}/{rup.Count50}/{rup.CountMiss}]\n";

                        if (rup.RankLetter == "F")
                        {
                            float total = rup.Count300 + rup.Count100 + rup.Count50 + rup.CountMiss;
                            float mapCompleted = (total / (result.TotalHitObjects + 1f) * 100f);
                            mapCompleted = MathF.Round(mapCompleted, 2);

                            temp += $"▸ **Map Completion:** {mapCompleted}%\n";
                        }

                        temp += $"▸ Score set {Utils.FormatTime(DateTime.UtcNow - rup.DateOfPlay)}\n";

                        if (!showList)
                        {
                            description += temp;
                            break;
                        }

                        if (temp.Length + description.Length < 2048)
                        {
                            description += temp;
                        }
                        else
                        {
                            count--;
                            break;
                        }
                    }

                    embedBuilder.WithThumbnailUrl(Utils.GetBeatmapImageUrl(beatmapSetID.ToString()));
                    embedBuilder.WithAuthor($"Recent plays for {userToCheck}", Utils.GetProfileImageUrl(recentUserPlays[0].UserID.ToString()));

                    embedBuilder.WithDescription(description);

                    embedBuilder.WithFooter($"Plays shown: {count} [{(int)(((float)sw.ElapsedTicks / Stopwatch.Frequency) * 1000f)} MS]");

                    embedBuilder.WithColor(new Color(Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255)));

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
            });
            //Kinda optimized
            CommandHandler.AddCommand(">top", (msg, sMsg) =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                if (sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You haven't set a profile, do >set <your_osu_name> to set your osu user");
                    return;
                }

                bool showRecent = false;
                string userToCheck = "";

                string[] user = sMsg.Content.Split(' ');
                if (user.Length > 2)
                {
                    userToCheck = user[1];

                    if (user[2].ToLower() == "-r")
                        showRecent = true;

                }
                else if (user.Length > 1)
                {
                    if (user[1].ToLower() == "-r")
                        showRecent = true;
                    else
                        userToCheck = user[1];
                }

                if (userToCheck == "")
                    userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                EmbedBuilder embedBuilder = new EmbedBuilder();
                string finalDescription = "";
                Logger.Log($"Getting best plays for '{userToCheck}'", LogLevel.Info);

                try
                {
                    List<OsuAPI.BestPlayResult> bestUserPlays = oApi.GetBestPlays(userToCheck);

                    List<OsuAPI.BestPlayResult> bestRecentUserPlays = new List<OsuAPI.BestPlayResult>(bestUserPlays);

                    OsuAPI.User osuUser = oApi.GetUser(userToCheck)[0];

                    if (showRecent)
                        bestRecentUserPlays.Sort((x, y) => DateTime.Compare(y.DateOfPlay, x.DateOfPlay));
                    else
                        bestUserPlays.Sort((x, y) => y.PP.CompareTo(x.PP));

                    embedBuilder.WithThumbnailUrl(Utils.GetProfileImageUrl(osuUser.ID.ToString()));

                    int count = 0;

                    for (int i = 0; i < 20; i++)
                    {
                        OsuAPI.BestPlayResult currentPlay;

                        if (showRecent)
                        {
                            currentPlay = bestRecentUserPlays[i];
                        }
                        else
                        {
                            currentPlay = bestUserPlays[i];
                        }

                        string map = Utils.DownloadBeatmap(currentPlay.BeatmapID.ToString());

                        EZPPResult result = EZPP.Calculate(map, currentPlay.MaxCombo, currentPlay.Count100, currentPlay.Count50, currentPlay.CountMiss, currentPlay.EnabledMods);

                        string temp = "";

                        temp += $"**{bestUserPlays.IndexOf(currentPlay) + 1}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(currentPlay.BeatmapID.ToString())}) **+{currentPlay.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★]\n";
                        temp += $"▸ {Utils.GetEmoteForRankLetter(currentPlay.RankLetter)} ▸ **{Math.Round(currentPlay.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}%\n";
                        temp += $"▸ {currentPlay.Score} ▸ x{currentPlay.MaxCombo}/{result.MaxCombo} ▸ [{currentPlay.Count300}/{currentPlay.Count100}/{currentPlay.Count50}/{currentPlay.CountMiss}]\n";
                        temp += $"▸ Score Set {Utils.FormatTime(DateTime.UtcNow - currentPlay.DateOfPlay)}\n";

                        if (finalDescription.Length + temp.Length < 2048)
                            finalDescription += temp;
                        else
                            break;

                        count = i + 1;
                    }

                    if (showRecent)
                        discordChannelToBeatmapTop.LazyAdd(sMsg.Channel.Id, bestRecentUserPlays.GetRange(0, count));
                    else
                        discordChannelToBeatmapTop.LazyAdd(sMsg.Channel.Id, bestUserPlays.GetRange(0, count));

                    if (showRecent)
                        embedBuilder.WithAuthor($"Top {count} Recent osu! Plays for {userToCheck}", Utils.GetFlagImageUrl(osuUser.Country));
                    else
                        embedBuilder.WithAuthor($"Top {count} osu! Plays for {userToCheck}", Utils.GetFlagImageUrl(osuUser.Country));

                    embedBuilder.WithDescription(finalDescription);

                    embedBuilder.WithFooter($"Plays shown: {count} [{(int)(((float)sw.ElapsedTicks / Stopwatch.Frequency) * 1000f)} MS]");

                    embedBuilder.WithColor(new Color(Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255)));

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());

                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Warning);
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }

                /*
                try
                {
                    if (userToCheck == "")
                        userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                    EmbedBuilder embedBuilder = new EmbedBuilder();
                    string description = "";
                    Console.WriteLine($"getting best plays for '{userToCheck}'");

                    List<OsuAPI.BestPlayResult> bestUserPlays = oApi.GetBestPlays(userToCheck);

                    List<OsuAPI.BestPlayResult> bestRecentUserPlays = new List<OsuAPI.BestPlayResult>(bestUserPlays);

                    OsuAPI.User osuUser = oApi.GetUser(userToCheck).First();

                    if (showRecent)
                        bestRecentUserPlays.Sort((x, y) => DateTime.Compare(y.DateOfPlay, x.DateOfPlay));
                    else
                        bestUserPlays.Sort((x, y) => y.PP.CompareTo(x.PP));

                    if (showRecent)
                        embedBuilder.WithAuthor($"Top 7 Recent osu! Plays for {userToCheck}", Utils.GetFlagImageUrl(osuUser.Country));
                    else
                        embedBuilder.WithAuthor($"Top 7 osu! Plays for {userToCheck}", Utils.GetFlagImageUrl(osuUser.Country));

                    embedBuilder.WithThumbnailUrl(Utils.GetProfileImageUrl(osuUser.ID.ToString()));

                    for (int i = 0; i < 7; i++)
                    {
                        OsuAPI.BestPlayResult currentPlay;

                        if (showRecent)
                        {
                            currentPlay = bestRecentUserPlays[i];
                        }
                        else
                        {
                            currentPlay = bestUserPlays[i];
                        }

                        string dateText = Utils.FormatTime(DateTime.UtcNow - currentPlay.DateOfPlay);

                        string map = Utils.DownloadBeatmap(currentPlay.BeatmapID.ToString());

                        EZPPResult result = EZPP.Calculate(map, currentPlay.MaxCombo, currentPlay.Count100, currentPlay.Count50, currentPlay.CountMiss, currentPlay.EnabledMods);

                        description += $"**{bestUserPlays.IndexOf(currentPlay) + 1}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(currentPlay.BeatmapID.ToString())}) **+{currentPlay.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★]\n";
                        description += $"▸ {Utils.GetEmoteForRankLetter(currentPlay.RankLetter)} ▸ **{Math.Round(currentPlay.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}%\n";
                        description += $"▸ {currentPlay.Score} ▸ x{currentPlay.MaxCombo}/{result.MaxCombo} ▸ [{currentPlay.Count300}/{currentPlay.Count100}/{currentPlay.Count50}/{currentPlay.CountMiss}]\n";
                        description += $"▸ Score Set {dateText}\n";
                    }

                    embedBuilder.WithDescription(description);

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());

                    if (showRecent)
                        discordChannelToBeatmapTop.LazyAdd(sMsg.Channel.Id, bestRecentUserPlays.GetRange(0, 7));
                    else
                        discordChannelToBeatmapTop.LazyAdd(sMsg.Channel.Id, bestUserPlays.GetRange(0, 7));
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Warning);
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
                */
            });

            CommandHandler.AddCommand(">set", (msg, sMsg) =>
            {
                string[] user = sMsg.Content.Split(' ');
                if (user.Length > 1)
                {
                    discordUserToOsuUser.LazySavableAdd(sMsg.Author.Id, user[1], "./OsuUsers");

                    sMsg.Channel.SendMessageAsync("Your osu user has been set to: " + user[1]);
                }
                else
                {
                    sMsg.Channel.SendMessageAsync("Please input a fucking user you idiot");
                }
            });

            //Optimized
            CommandHandler.AddCommand(">c", (msg, sMsg) =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                if (sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You haven't set a profile, do >set <your_osu_name> to set your osu user");
                    return;
                }

                if (!discordChannelToBeatmap.ContainsKey(sMsg.Channel.Id))
                {
                    sMsg.Channel.SendMessageAsync("No beatmap found in conversation");
                    return;
                }

                string userToCheck = "";

                string[] args = sMsg.Content.Split(' ');

                if (args.Length > 1)
                    userToCheck = args[1];

                if (userToCheck == "")
                    userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                EmbedBuilder embedBuilder = new EmbedBuilder();
                string finalDescription = "";
                Logger.Log($"getting user plays for '{userToCheck}'", LogLevel.Info);

                try
                {
                    List<OsuAPI.ScoreResult> userPlays = oApi.GetScores(userToCheck, discordChannelToBeatmap[sMsg.Channel.Id]);

                    if (userPlays.Count == 0)
                    {
                        sMsg.Channel.SendMessageAsync($"**{userToCheck} don't have any plays on this map** <:sadChamp:593405356864962560>");
                        return;
                    }

                    embedBuilder.WithAuthor($"Top osu! Plays for {userToCheck}", Utils.GetProfileImageUrl(userPlays[0].UserID.ToString()));

                    string map = Utils.DownloadBeatmap(discordChannelToBeatmap[sMsg.Channel.Id].ToString());

                    string beatmapSetID = Utils.FindBeatmapSetID(map).ToString();

                    embedBuilder.WithThumbnailUrl(Utils.GetBeatmapImageUrl(beatmapSetID));

                    int count = 0;
                    foreach (var play in userPlays)
                    {
                        EZPPResult result = EZPP.Calculate(map, play.MaxCombo, play.Count100, play.Count50, play.CountMiss, play.EnabledMods);

                        string tempDescription = "";

                        tempDescription += $"**{++count}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(discordChannelToBeatmap[sMsg.Channel.Id].ToString())}) **+{play.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★]\n";
                        tempDescription += $"▸ {Utils.GetEmoteForRankLetter(play.RankLetter)} ▸ **{Math.Round(result.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}%\n";
                        tempDescription += $"▸ {play.Score} ▸ x{play.MaxCombo}/{result.MaxCombo} ▸ [{play.Count300}/{play.Count100}/{play.Count50}/{play.CountMiss}]\n";
                        tempDescription += $"▸ Score Set {Utils.FormatTime(DateTime.UtcNow - play.DateOfPlay)}\n";

                        if (finalDescription.Length + tempDescription.Length < 2048)
                            finalDescription += tempDescription;
                        else
                            break;
                    }

                    embedBuilder.WithDescription(finalDescription);
                    embedBuilder.WithFooter($"Plays shown: {count} [{(int)(((float)sw.ElapsedTicks / Stopwatch.Frequency) * 1000f)} MS]");
                    embedBuilder.WithColor(new Color(Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255)));

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Warning);
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
            });
            //Kinda deprecated by >match
            CommandHandler.AddCommand(">flex", (msg, sMsg) =>
            {
                if (sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You have not setup your osu user, do: >set <username>");
                    return;
                }

                if (!discordChannelToBeatmapTop.ContainsKey(sMsg.Channel.Id))
                {
                    sMsg.Channel.SendMessageAsync("No top plays found in conversation");
                    return;
                }

                string userToCheck = "";

                int indexTopToCompare = 0;

            string[] user = sMsg.Content.Split(' ');
            if (user.Length > 2)
            {
                if (!int.TryParse(user[1], out indexTopToCompare))
                {
                    sMsg.Channel.SendMessageAsync("invalid arguments [>flex <user> <index>] or [>flex <index>]");
                    return;
                }

                userToCheck = user[2];
            }
            else if (user.Length > 1)
            {
                if (!int.TryParse(user[1], out indexTopToCompare))
                {
                    sMsg.Channel.SendMessageAsync("invalid arguments [>flex <user> <index>] or [>flex <index>]");
                    return;
                }
            }

                if (userToCheck == "")
                    userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                if (indexTopToCompare > discordChannelToBeatmapTop[sMsg.Channel.Id].Count)
                    indexTopToCompare = 7;

                if (indexTopToCompare < 1)
                    indexTopToCompare = 1;

                //Subtract one to get the actual index in the list
                ulong beatmapToCheck = discordChannelToBeatmapTop[sMsg.Channel.Id][indexTopToCompare - 1].BeatmapID;

                try
                {
                    OsuAPI.User osuUser = oApi.GetUser(userToCheck).First();

                    List<OsuAPI.ScoreResult> userPlay = oApi.GetScores(userToCheck, beatmapToCheck);

                    if (userPlay.Count == 0)
                    {
                        sMsg.Channel.SendMessageAsync($"**{userToCheck} don't have any plays on this map** <:sadChamp:593405356864962560>");
                        return;
                    }

                    if (discordChannelToBeatmap.ContainsKey(sMsg.Channel.Id))
                    {
                        discordChannelToBeatmap[sMsg.Channel.Id] = beatmapToCheck;
                    }
                    else
                    {
                        discordChannelToBeatmap.Add(sMsg.Channel.Id, beatmapToCheck);
                    }

                    EmbedBuilder embedBuilder = new EmbedBuilder();
                    string description = "";
                    Console.WriteLine($"getting user plays for '{userToCheck}'");

                    embedBuilder.WithAuthor($"Top compared osu! Plays for {userToCheck}", Utils.GetFlagImageUrl(osuUser.Country));

                    embedBuilder.WithThumbnailUrl(Utils.GetProfileImageUrl(osuUser.ID.ToString()));

                    try
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            var currentPlay = userPlay[i];
                            string dateText = Utils.FormatTime(DateTime.UtcNow - currentPlay.DateOfPlay);

                            string map = Utils.DownloadBeatmap(beatmapToCheck.ToString());

                            EZPPResult result = EZPP.Calculate(map, currentPlay.MaxCombo, currentPlay.Count100, currentPlay.Count50, currentPlay.CountMiss, currentPlay.EnabledMods);

                            description += $"**{i + 1}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(beatmapToCheck.ToString())}) **+{currentPlay.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★]\n";
                            description += $"▸ {Utils.GetEmoteForRankLetter(currentPlay.RankLetter)} ▸ **{Math.Round(result.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}%\n";
                            description += $"▸ {currentPlay.Score} ▸ x{currentPlay.MaxCombo}/{result.MaxCombo} ▸ [{currentPlay.Count300}/{currentPlay.Count100}/{currentPlay.Count50}/{currentPlay.CountMiss}]\n";
                            description += $"▸ Score Set {dateText}\n";
                        }
                    }
                    catch
                    {

                    }
                    embedBuilder.WithDescription(description);

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Warning);
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
            });

            //Optimized
            CommandHandler.AddCommand(">osu", (msg, sMsg) =>
            {
                if (sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You haven't set a profile, do >set <your_osu_name> to set your osu user");
                    return;
                }

                string userToCheck = "";

                string[] args = sMsg.Content.Split(' ');

                if (args.Length > 1)
                    userToCheck = args[1];
                else
                    userToCheck = discordUserToOsuUser[sMsg.Author.Id];

                try
                {
                    OsuAPI.User user = oApi.GetUser(userToCheck).First();

                    EmbedBuilder embedBuilder = new EmbedBuilder();

                    embedBuilder.WithAuthor($"osu! Profile For {userToCheck}", Utils.GetFlagImageUrl(user.Country));

                    embedBuilder.Description = $"▸ **Official Rank:** #{user.Rank} ({user.Country}#{user.CountryRank})\n▸ **Level:** {user.Level}\n▸ **Total PP:** {user.PP}\n▸ **Hit Accuracy:** {Math.Round(user.Accuracy, 2)}%\n▸ **Playcount:** {user.Playcount}";
                    embedBuilder.WithThumbnailUrl(Utils.GetProfileImageUrl(user.ID.ToString()));

                    if (userToCheck.ToLower() == "raresica1234")
                        embedBuilder.WithFooter("😎 Bot Helper");

                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
            });
            //Optimized
            CommandHandler.AddCommand(">match", (msg, sMsg) =>
            {
                Stopwatch sw = Stopwatch.StartNew();

                if(sMsg.MentionedUsers.Count > 0)
                {
                    sMsg.Channel.SendMessageAsync("Really? Please don't tag people, just you their osu username. Thanks");
                    return;
                }

                if (!discordUserToOsuUser.ContainsKey(sMsg.Author.Id))
                {
                    sMsg.Channel.SendMessageAsync("You have not setup your osu user, do: >set <username>");
                    return;
                }

                string userToCheck = "";

                string[] args = sMsg.Content.Split(' ');

                if (args.Length > 1)
                    userToCheck = args[1];
                else
                {
                    sMsg.Channel.SendMessageAsync("Use >match <user>");
                    return;
                }

                try
                {
                    List<OsuAPI.BestPlayResult> firstTopPlays = oApi.GetBestPlays(discordUserToOsuUser[sMsg.Author.Id]);
                    List<OsuAPI.BestPlayResult> secondTopPlays = oApi.GetBestPlays(userToCheck);

                    OsuAPI.User firstUser = oApi.GetUser(discordUserToOsuUser[sMsg.Author.Id]).First();

                    OsuAPI.User secondUser = oApi.GetUser(userToCheck).First();

                    EmbedBuilder embedBuilder = new EmbedBuilder();

                    embedBuilder.WithAuthor($"Matching Top Plays For {discordUserToOsuUser[sMsg.Author.Id]} vs {userToCheck}", Utils.GetProfileImageUrl(firstUser.ID.ToString()));

                    embedBuilder.WithThumbnailUrl(Utils.GetProfileImageUrl(secondUser.ID.ToString()));

                    string description = "";

                    int count = 0;

                    for (int i = 0; i < firstTopPlays.Count; i++)
                    {
                        if (secondTopPlays.Any(x => x.BeatmapID == firstTopPlays[i].BeatmapID))
                        {
                            count++;

                            OsuAPI.BestPlayResult currentBestPlay = firstTopPlays[i];

                            string dateText = Utils.FormatTime(DateTime.UtcNow - currentBestPlay.DateOfPlay);

                            string map = Utils.DownloadBeatmap(currentBestPlay.BeatmapID.ToString());

                            EZPPResult result = EZPP.Calculate(map, currentBestPlay.MaxCombo, currentBestPlay.Count100, currentBestPlay.Count50, currentBestPlay.CountMiss, currentBestPlay.EnabledMods);

                            string tempDescription = "";

                            tempDescription += $"**{count}.** [**{result.SongName} [{result.DifficultyName}]**]({Utils.GetBeatmapUrl(currentBestPlay.BeatmapID.ToString())})\n";
                            tempDescription += $"▸ {Utils.GetEmoteForRankLetter(currentBestPlay.RankLetter)} ▸ **{Math.Round(currentBestPlay.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}% **+{currentBestPlay.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★] ▸ **#{i + 1}**\n";
                            tempDescription += $"▸ {currentBestPlay.Score} ▸ x{currentBestPlay.MaxCombo}/{currentBestPlay.MaxCombo} ▸ [{currentBestPlay.Count300}/{currentBestPlay.Count100}/{currentBestPlay.Count50}/{currentBestPlay.CountMiss}]\n";
                            tempDescription += $"▸ Score Set {dateText}\n";

                            tempDescription += $"▸ **VS**\n";

                            int index = secondTopPlays.FindIndex(x => x.BeatmapID == firstTopPlays[i].BeatmapID);

                            currentBestPlay = secondTopPlays[index];

                            dateText = Utils.FormatTime(DateTime.UtcNow - currentBestPlay.DateOfPlay);

                            result = EZPP.Calculate(map, currentBestPlay.MaxCombo, currentBestPlay.Count100, currentBestPlay.Count50, currentBestPlay.CountMiss, currentBestPlay.EnabledMods);

                            tempDescription += $"▸ {Utils.GetEmoteForRankLetter(currentBestPlay.RankLetter)} ▸ **{Math.Round(currentBestPlay.PP, 2)}pp** ▸ {Math.Round(result.Accuracy, 2)}% **+{currentBestPlay.EnabledMods.ToFriendlyString()}** [{Math.Round(result.StarRating, 2)}★] ▸ **#{index + 1}**\n";
                            tempDescription += $"▸ {currentBestPlay.Score} ▸ x{currentBestPlay.MaxCombo}/{currentBestPlay.MaxCombo} ▸ [{currentBestPlay.Count300}/{currentBestPlay.Count100}/{currentBestPlay.Count50}/{currentBestPlay.CountMiss}]\n";
                            tempDescription += $"▸ Score Set {dateText}\n";

                            tempDescription += " \n";

                            if (description.Length + tempDescription.Length < 2048)
                            {
                                description += tempDescription;
                            }
                            else
                            {
                                break;
                            }
                        }

                    }

                    embedBuilder.WithDescription(description);
                    embedBuilder.WithFooter($"Plays shown: {count} [{(int)(((float)sw.ElapsedTicks / Stopwatch.Frequency) * 1000f)} MS]");
                    embedBuilder.WithColor(new Color(Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255), Utils.GetRandomNumber(0, 255)));
                    sMsg.Channel.SendMessageAsync("", false, embedBuilder.Build());
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, LogLevel.Warning);
                    Logger.Log(ex.StackTrace, LogLevel.Error);
                    sMsg.Channel.SendMessageAsync("uh oh something happend check console");
                }
            });

        }

        public bool HandleMessage(SocketMessage msg)
        {
            return false;
        }

        public void Unloading()
        {
            CommandHandler.RemoveCommand(">rs");
            CommandHandler.RemoveCommand(">top");
            CommandHandler.RemoveCommand(">set");
            CommandHandler.RemoveCommand(">c");
            CommandHandler.RemoveCommand(">flex");
            CommandHandler.RemoveCommand(">match");
            CommandHandler.RemoveCommand(">osu");
        }
    }
}
