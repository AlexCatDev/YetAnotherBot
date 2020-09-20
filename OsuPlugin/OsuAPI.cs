using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OsuPlugin
{
    public class OsuAPI
    {
        private string apiKey;

        public OsuAPI(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public List<RecentPlayResult> GetRecentPlays(string username, int limit = 1)
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString($"https://osu.ppy.sh/api/get_user_recent?k={apiKey}&u={username}&m=0&limit={limit}&type=string");
                return JsonConvert.DeserializeObject<List<RecentPlayResult>>(json);
            }
        }

        public List<User> GetUser(string username)
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString($"https://osu.ppy.sh/api/get_user?k={apiKey}&u={username}&m=0&type=string");
                return JsonConvert.DeserializeObject<List<User>>(json);
            }
        }

        public List<BestPlayResult> GetBestPlays(string username)
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString($"https://osu.ppy.sh/api/get_user_best?k={apiKey}&u={username}&m=0&limit=100&type=string");
                return JsonConvert.DeserializeObject<List<BestPlayResult>>(json);
            }
        }

        public List<ScoreResult> GetScores(string username, ulong beatmapID, int limit = 10)
        {
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString($"https://osu.ppy.sh/api/get_scores?k={apiKey}&b={beatmapID}&u={username}&m=0&limit={limit}&type=string");
                return JsonConvert.DeserializeObject<List<ScoreResult>>(json);
            }
        }

        public class User
        {
            [JsonProperty("playcount")]
            public int Playcount;

            [JsonProperty("pp_rank")]
            public int Rank;

            [JsonProperty("level")]
            public float Level;

            [JsonProperty("accuracy")]
            public float Accuracy;

            [JsonProperty("country")]
            public string Country;

            [JsonProperty("pp_country_rank")]
            public int CountryRank;

            [JsonProperty("user_id")]
            public ulong ID;

            [JsonProperty("pp_raw")]
            public float PP;
        }

        public class ScoreResult
        {
            [JsonProperty("score_id")]
            public ulong ScoreID;
            [JsonProperty("score")]
            public int Score;
            [JsonProperty("username")]
            public string Username;
            [JsonProperty("count300")]
            public int Count300;
            [JsonProperty("count100")]
            public int Count100;
            [JsonProperty("count50")]
            public int Count50;
            [JsonProperty("countmiss")]
            public int CountMiss;
            [JsonProperty("maxcombo")]
            public int MaxCombo;
            [JsonProperty("countkatu")]
            public int CountKatu;
            [JsonProperty("countgeki")]
            public int CountGeki;
            [JsonProperty("perfect")]
            public int Perfect;
            [JsonProperty("enabled_mods")]
            public Mods EnabledMods;
            [JsonProperty("user_id")]
            public ulong UserID;
            [JsonProperty("date")]
            public DateTime DateOfPlay;
            [JsonProperty("rank")]
            public string RankLetter;
            [JsonProperty("pp")]
            public float? PP;
            [JsonProperty("replay_available")]
            public int ReplayAvailable;
        }

        public class BestPlayResult
        {
            [JsonProperty("beatmap_id")]
            public ulong BeatmapID;
            [JsonProperty("score_id")]
            public ulong ScoreID;
            [JsonProperty("score")]
            public int Score;
            [JsonProperty("maxcombo")]
            public int MaxCombo;
            [JsonProperty("count50")]
            public int Count50;
            [JsonProperty("count100")]
            public int Count100;
            [JsonProperty("count300")]
            public int Count300;
            [JsonProperty("countmiss")]
            public int CountMiss;
            [JsonProperty("countkatu")]
            public int CountKatu;
            [JsonProperty("countgeki")]
            public int CountGeki;
            [JsonProperty("perfect")]
            public int Perfect;
            [JsonProperty("enabled_mods")]
            public Mods EnabledMods;
            [JsonProperty("user_id")]
            public ulong UserID;
            [JsonProperty("date")]
            public DateTime DateOfPlay;
            [JsonProperty("rank")]
            public string RankLetter;
            [JsonProperty("pp")]
            public float PP;
            [JsonProperty("replay_available")]
            public int ReplayAvailable;
        }

        public class RecentPlayResult
        {
            [JsonProperty("beatmap_id")]
            public ulong BeatmapID;
            [JsonProperty("score")]
            public int Score;
            [JsonProperty("maxcombo")]
            public int MaxCombo;
            [JsonProperty("count50")]
            public int Count50;
            [JsonProperty("count100")]
            public int Count100;
            [JsonProperty("count300")]
            public int Count300;
            [JsonProperty("countmiss")]
            public int CountMiss;
            [JsonProperty("countkatu")]
            public int CountKatu;
            [JsonProperty("countgeki")]
            public int CountGeki;
            [JsonProperty("perfect")]
            public int Perfect;
            [JsonProperty("enabled_mods")]
            public Mods EnabledMods;
            [JsonProperty("user_id")]
            public ulong UserID;
            [JsonProperty("date")]
            public DateTime DateOfPlay;
            [JsonProperty("rank")]
            public string RankLetter;
        }
    }
}
