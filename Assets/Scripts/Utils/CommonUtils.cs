using System.Collections.Generic;
using Common;
using Services;
using Storage;
using UnityEngine;

namespace Utils
{
    public static class CommonUtils
    {
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            System.Random rng = new System.Random();
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
    }
}