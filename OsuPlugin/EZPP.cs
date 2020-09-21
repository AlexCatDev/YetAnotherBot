using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OsuPlugin
{
    public static class EZPP
    {
        [DllImport(@"oppai.dll")]
        private static extern IntPtr ezpp_new();

        [DllImport(@"oppai.dll")]
        private static extern void ezpp_free(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern IntPtr ezpp_data_dup(IntPtr ez, string data, int dataSize);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_pp(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_set_mods(IntPtr ez, int mods);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_set_combo(IntPtr ez, int combo);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_set_nmiss(IntPtr ez, int miss);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_stars(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern float ezpp_accuracy_percent(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern void ezpp_set_accuracy(IntPtr ez, int n100, int n50);

        [DllImport(@"oppai.dll")]
        private static extern int ezpp_max_combo(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern IntPtr ezpp_version(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern IntPtr ezpp_title(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern int ezpp_nobjects(IntPtr ez);

        [DllImport(@"oppai.dll")]
        private static extern void ezpp_set_accuracy_percent(IntPtr ez, float accuracy_percent);

        private static IntPtr ezppInstance;
        private static object ezppLock = new object();

        public static EZPPResult Calculate(string mapData, int maxCombo, int n100, int n50, int nMisses, Mods mods)
        {
            lock (ezppLock)
            {
                //Create ezpp instance
                ezppInstance = ezpp_new();

                EZPPResult result = new EZPPResult();

                //give mods to ezpp
                ezpp_set_mods(ezppInstance, (int)mods);

                //give 100's and 50's to ezpp
                ezpp_set_accuracy(ezppInstance, n100, n50);

                //give misscount to ezpp
                ezpp_set_nmiss(ezppInstance, nMisses);

                //give max combo to ezpp
                ezpp_set_combo(ezppInstance, maxCombo);

                //give a duplicate of map data to ezpp, so it can calculate above values ^^
                ezpp_data_dup(ezppInstance, mapData, mapData.Length);

                //Calculate pp first
                result.PP = ezpp_pp(ezppInstance);

                result.StarRating = ezpp_stars(ezppInstance);

                result.Accuracy = ezpp_accuracy_percent(ezppInstance);

                result.MaxCombo = ezpp_max_combo(ezppInstance);

                result.TotalHitObjects = ezpp_nobjects(ezppInstance);

                result.DifficultyName = ConvertUnsafeCString(ezpp_version(ezppInstance));

                result.SongName = ConvertUnsafeCString(ezpp_title(ezppInstance));

                //Destroy ezpp
                ezpp_free(ezppInstance);

                return result;
            }
        }

        public static EZPPResult Calculate(string mapData, int maxCombo, float accuracy, int nMisses, Mods mods)
        {
            lock (ezppLock)
            {
                //Create ezpp instance
                ezppInstance = ezpp_new();

                EZPPResult result = new EZPPResult();

                //give mods to ezpp
                ezpp_set_mods(ezppInstance, (int)mods);

                //give accuracy_percentage to ezpp
                ezpp_set_accuracy_percent(ezppInstance, accuracy);

                //give misscount to ezpp
                ezpp_set_nmiss(ezppInstance, nMisses);

                //give max combo to ezpp
                ezpp_set_combo(ezppInstance, maxCombo);

                //give a duplicate of map data to ezpp, so it can calculate above values ^^
                ezpp_data_dup(ezppInstance, mapData, mapData.Length);

                //Calculate pp first
                result.PP = ezpp_pp(ezppInstance);

                result.StarRating = ezpp_stars(ezppInstance);

                result.Accuracy = ezpp_accuracy_percent(ezppInstance);

                result.MaxCombo = ezpp_max_combo(ezppInstance);

                result.TotalHitObjects = ezpp_nobjects(ezppInstance);

                result.DifficultyName = ConvertUnsafeCString(ezpp_version(ezppInstance));

                result.SongName = ConvertUnsafeCString(ezpp_title(ezppInstance));

                //Destroy ezpp
                ezpp_free(ezppInstance);

                return result;
            }
        }

        private unsafe static string ConvertUnsafeCString(IntPtr ptr)
        {
            void* rawPointer = ptr.ToPointer();
            if (rawPointer == null) return "";

            StringBuilder b = new StringBuilder();

            byte* unsafeString = (byte*)rawPointer;

            int offset = 0;

            while (unsafeString[offset] != 0)
            {
                b.Append((char)Marshal.ReadByte(ptr, offset++));
                
            }

            return b.ToString();
        }
    }

    public class EZPPResult
    {
        public float StarRating;
        public float Accuracy;
        public float PP;

        public string DifficultyName;
        public int MaxCombo;

        public int TotalHitObjects;

        public string SongName;
    }
}
