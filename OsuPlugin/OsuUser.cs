using System;
using System.Collections.Generic;
using System.Text;

namespace OsuPlugin
{
    public class OsuUser
    {
        public int Playcount { get; private set; }

        public int Rank { get; private set; }

        public float Level { get; private set; }

        public float Accuracy { get; private set; }

        public string Country { get; private set; }

        public int CountryRank { get; private set; }

        public ulong ID { get; private set; }

        public float PP { get; private set; }

        public OsuUser(BanchoAPI.BanchoUser banchoUser)
        {
            Playcount = banchoUser.Playcount;
            Rank = banchoUser.Rank;
            Level = MathF.Round(banchoUser.Level, 2);
            Accuracy = MathF.Round(banchoUser.Accuracy, 2);
            Country = banchoUser.Country;
            CountryRank = banchoUser.CountryRank;
            ID = banchoUser.ID;
            PP = MathF.Round(banchoUser.PP, 2);
        }
    }
}
