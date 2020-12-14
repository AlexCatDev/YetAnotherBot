using Discord;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace OsuPlugin
{
    public static class Utils
    {
        public static bool BeatmapCachingEnabled = true;

        private static Dictionary<string, string> cachedBeatmaps = new Dictionary<string, string>();

        public static int CachedBeatmapCount => cachedBeatmaps.Count;

        private static Random rng = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            return rng.Next(min, max + 1);
        }

        public static string ToFriendlyString(this Mods mod)
        {
            string output = mod.ToString().Replace(", ", "");

            if (mod.HasFlag(Mods.NC))
                output = output.Replace("DT", "");

            return output;
        }

        public static string DownloadBeatmap(string beatmapID)
        {
            string beatmap = "";
            if (cachedBeatmaps.TryGetValue(beatmapID, out beatmap))
            {
                return beatmap;
            }
            else
            {
                using (WebClient wc = new WebClient())
                {
                    beatmap = wc.DownloadString($"https://osu.ppy.sh/osu/{beatmapID}");

                    if(BeatmapCachingEnabled)
                        cachedBeatmaps.Add(beatmapID, beatmap);

                    return beatmap;
                }
            }
        }

        public static string GetProfileImageUrl(string userID) => $"https://a.ppy.sh/{userID}?{GetRandomNumber(1, 36487264)}.jpeg";
        public static string GetBeatmapImageUrl(string beatmapSetID) => $"https://b.ppy.sh/thumb/{beatmapSetID}l.jpg";
        public static string GetFlagImageUrl(string country) => $"https://osu.ppy.sh/images/flags/{country}.png";

        public static string GetBeatmapUrl(string beatmapID) => $"https://osu.ppy.sh/b/{beatmapID}";

        public static int FindBeatmapSetID(string mapData)
        {
            int index = mapData.ToLower().IndexOf("beatmapsetid:");

            int offset = index + "beatmapsetid:".Length;

            string beatmapset = "";

            while (mapData[offset] != '\r')
            {
                beatmapset += mapData[offset++];
            }

            int result = 0;
            if (int.TryParse(beatmapset, out result))
                return result;
            else
                return 0;
        }

        public static void LazyAdd(this Dictionary<ulong, List<BanchoAPI.BanchoBestScore>> dict, ulong key, List<BanchoAPI.BanchoBestScore> value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public static void LazySavableAdd(this Dictionary<ulong, string> dict, ulong key, string value, string savePath)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }

            string json = JsonConvert.SerializeObject(dict);

            File.WriteAllText(savePath, json);
        }

        //Credits: raresica1234
        public static string FormatTime(TimeSpan time)
        {
            int statCount = 3;
            int[] stats = new int[] { (int)(time.Days / 365), 0, time.Days, time.Hours, time.Minutes, time.Seconds < 1 ? 1 : time.Seconds };
            stats[2] -= stats[0] * 365;
            stats[1] =  stats[2] / 30;
            stats[2] -= stats[1] * 30;
            string[] names = new string[] { " Years ", " Months ", " Days ", " Hours ", " Minutes ", " Seconds " };
            string output = "";
            for (int i = 0; i < stats.Length && statCount != 0; i++)
            {
                if (stats[i] != 0)
                {
                    output += stats[i] + names[i];
                    statCount--;
                }
            }

            output += "Ago";

            return output;
        }

        public static string GetEmoteForRankLetter(string rankLetter)
        {
            switch (rankLetter)
            {
                case "F":
                    return "<:rankF:756892070177931407>";
                case "D":
                    return "<:rankD:756833574342098994>";
                case "C":
                    return "<:rankC:756833574123995218>";
                case "B":
                    return "<:rankB:756833574296092672>";
                case "A":
                    return "<:rankA:756833574228983958>";
                case "S":
                    return "<:rankS:756833574216400896>";
                case "X":
                    return "<:rankSS:756833574228852796>";
                case "SH":
                    return "<:rankSH:756833574187171880>";
                case "XH":
                    return "<:rankSSH:756833574111674390>";
                default:
                    return ":sunglasses:";
            }
        }
    }
}
