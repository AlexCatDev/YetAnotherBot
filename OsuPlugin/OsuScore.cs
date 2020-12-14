using System;
using System.Collections.Generic;
using System.Text;

namespace OsuPlugin
{
    public class OsuScore
    {
        public DateTime Date { get; private set; }

        public int Count300 { get; private set; }
        public int Count100 { get; private set; }
        public int Count50 { get; private set; }
        public int CountMiss { get; private set; }

        public int Score { get; private set; }

        public int MaxCombo { get; private set; }

        public int MapMaxCombo { get; private set; }

        public Mods EnabledMods { get; private set; } 

        public double PP { get; private set; }
        public double Accuracy { get; private set; }

        public double PP_IF_FC { get; private set; }
        public double IF_FC_Accuracy { get; private set; }

        public double MapCompletionPercentage { get; private set; }

        public string RankingLetter { get; private set; }

        public ulong BeatmapID { get; private set; }

        public float StarRating { get; private set; }
        public float CS { get; private set; }
        public float AR { get; private set; }
        public float OD { get; private set; }
        public float HP { get; private set; }
        public float BPM { get; private set; }

        public string SongName { get; private set; }
        public string DifficultyName { get; private set; }

        public OsuScore(BanchoAPI.BanchoScore banchoPlay, ulong beatmapID)
        {
            Date = banchoPlay.DateOfPlay;
            Count300 = banchoPlay.Count300;
            Count100 = banchoPlay.Count100;
            Count50 = banchoPlay.Count50;
            CountMiss = banchoPlay.CountMiss;

            Score = banchoPlay.Score;

            MaxCombo = banchoPlay.MaxCombo;

            EnabledMods = banchoPlay.EnabledMods;

            string beatmap = Utils.DownloadBeatmap(beatmapID.ToString());
            EZPPResult ezpp = EZPP.Calculate(beatmap, MaxCombo, Count100, Count50, CountMiss, EnabledMods);

            MapMaxCombo = ezpp.MaxCombo;

            PP = ezpp.PP;
            Accuracy = ezpp.Accuracy;

            float objectsEncountered = Count300 + Count100 + Count50 + CountMiss;
            float percentage100 = ((float)Count100 + CountMiss) / objectsEncountered;
            float percentage50 = Count50 / objectsEncountered;
            int expected100 = (int)(ezpp.TotalHitObjects * percentage100);
            int expected50 = (int)(ezpp.TotalHitObjects * percentage50);

            ezpp = EZPP.Calculate(beatmap, ezpp.MaxCombo, expected100, expected50, 0, EnabledMods);

            PP_IF_FC = ezpp.PP;
            IF_FC_Accuracy = ezpp.Accuracy;

            MapCompletionPercentage = MathF.Round((objectsEncountered / (ezpp.TotalHitObjects + 1f) * 100f), 2);

            RankingLetter = banchoPlay.RankLetter;

            BeatmapID = beatmapID;

            StarRating = ezpp.StarRating;
            CS = ezpp.CS;
            AR = ezpp.AR;
            OD = ezpp.OD;
            HP = ezpp.HP;
            BPM = ezpp.BPM;

            SongName = ezpp.SongName;
            DifficultyName = ezpp.DifficultyName;
        }

        public OsuScore(BanchoAPI.BanchoBestScore banchoPlay)
        {
            Date = banchoPlay.DateOfPlay;
            Count300 = banchoPlay.Count300;
            Count100 = banchoPlay.Count100;
            Count50 = banchoPlay.Count50;
            CountMiss = banchoPlay.CountMiss;

            Score = banchoPlay.Score;

            MaxCombo = banchoPlay.MaxCombo;

            EnabledMods = banchoPlay.EnabledMods;

            string beatmap = Utils.DownloadBeatmap(banchoPlay.BeatmapID.ToString());
            EZPPResult ezpp = EZPP.Calculate(beatmap, MaxCombo, Count100, Count50, CountMiss, EnabledMods);

            MapMaxCombo = ezpp.MaxCombo;

            PP = MathF.Round(banchoPlay.PP, 2);
            Accuracy = ezpp.Accuracy;

            float objectsEncountered = Count300 + Count100 + Count50 + CountMiss;
            float percentage100 = ((float)Count100 + CountMiss) / objectsEncountered;
            float percentage50 = Count50 / objectsEncountered;
            int expected100 = (int)(ezpp.TotalHitObjects * percentage100);
            int expected50 = (int)(ezpp.TotalHitObjects * percentage50);

            ezpp = EZPP.Calculate(beatmap, ezpp.MaxCombo, expected100, expected50, 0, EnabledMods);

            PP_IF_FC = ezpp.PP;
            IF_FC_Accuracy = ezpp.Accuracy;

            RankingLetter = banchoPlay.RankLetter;

            BeatmapID = banchoPlay.BeatmapID;

            StarRating = ezpp.StarRating;
            CS = ezpp.CS;
            AR = ezpp.AR;
            OD = ezpp.OD;
            HP = ezpp.HP;
            BPM = ezpp.BPM;

            SongName = ezpp.SongName;
            DifficultyName = ezpp.DifficultyName;
        }

        public OsuScore(BanchoAPI.BanchoRecentScore banchoPlay)
        {
            Date = banchoPlay.DateOfPlay;
            Count300 = banchoPlay.Count300;
            Count100 = banchoPlay.Count100;
            Count50 = banchoPlay.Count50;
            CountMiss = banchoPlay.CountMiss;

            Score = banchoPlay.Score;

            MaxCombo = banchoPlay.MaxCombo;

            EnabledMods = banchoPlay.EnabledMods;

            string beatmap = Utils.DownloadBeatmap(banchoPlay.BeatmapID.ToString());
            EZPPResult ezpp = EZPP.Calculate(beatmap, MaxCombo, Count100, Count50, CountMiss, EnabledMods);

            MapMaxCombo = ezpp.MaxCombo;

            PP = ezpp.PP;
            Accuracy = ezpp.Accuracy;

            float objectsEncountered = Count300 + Count100 + Count50 + CountMiss;
            float percentage100 = ((float)Count100 + CountMiss) / objectsEncountered;
            float percentage50 = Count50 / objectsEncountered;
            int expected100 = (int)(ezpp.TotalHitObjects * percentage100);
            int expected50 = (int)(ezpp.TotalHitObjects * percentage50);

            ezpp = EZPP.Calculate(beatmap, ezpp.MaxCombo, expected100, expected50, 0, EnabledMods);

            PP_IF_FC = ezpp.PP;
            IF_FC_Accuracy = ezpp.Accuracy;

            RankingLetter = banchoPlay.RankLetter;

            BeatmapID = banchoPlay.BeatmapID;

            StarRating = ezpp.StarRating;
            CS = ezpp.CS;
            AR = ezpp.AR;
            OD = ezpp.OD;
            HP = ezpp.HP;
            BPM = ezpp.BPM;

            SongName = ezpp.SongName;
            DifficultyName = ezpp.DifficultyName;
        }
    }
}
