using System.Collections.Generic;
using Common;
using Services;
using UnityEngine;

namespace Utils
{
    public static class CommonUtils
    {
        public static IList<T> Shuffle<T>(IList<T> list, int seed)
        {
            System.Random rng = new System.Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }

        public static Color GetUIImageColor(SolitaireKind kind)
        {
            return God.Settings.Config.GetUIImageColor(kind);
        }

        public static Color GetUITextColor(SolitaireKind kind)
        {
            return God.Settings.Config.GetUITextColor(kind);
        }

        public static Color GetWinsColor(int wins)
        {
            return God.Settings.Config.GetWinsColor(wins);
        }

        internal static string GetRules(SolitaireKind kind)
        {
            return God.Settings.Config.GetRules(kind);
        }

        internal static string GetTouchCtrlsText()
        {
            return God.Settings.Config.GetTouchCtrlsDesc();
        }

        internal static string GetKeyboardMouseCtrlsText()
        {
            return God.Settings.Config.GetKeyboardMouseCtrlsDesc();
        }

        internal static string ToString(int[] intarray)
        {
            return string.Join(',', intarray);
        }

        internal static int[] FromString(string intarray)
        {
            if (string.IsNullOrWhiteSpace(intarray))
            {
                return System.Array.Empty<int>();
            }

            string[] parts = intarray.Split(',');
            int[] result = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                int.TryParse(parts[i], out result[i]);
            }

            return result;
        }
    }
}