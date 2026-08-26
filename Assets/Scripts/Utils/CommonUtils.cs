using System;
using System.Collections.Generic;
using Common;
using FreeCell;
using Klondike;
using Sawayama;
using Spider;
using UnityEngine;

namespace Utils
{
    public static class CommonUtils
    {
        static Configs Config;

        public static void LoadConfig(Configs config)
        {
            Config = config;
        }

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
            return Config.GetUIImageColor(kind);
        }

        public static Color GetUITextColor(SolitaireKind kind)
        {
            return Config.GetUITextColor(kind);
        }

        public static int GetWinsFor(SolitaireKind solitaireKind)
        {
            switch (solitaireKind)
            {
                case SolitaireKind.KLONDIKE:
                    return KlondikeSettings.WinCount;

                case SolitaireKind.SAWAYAMA:
                    return SawayamaSettings.WinCount;

                case SolitaireKind.FREECELL:
                    return FreeCellGameSettings.WinCount;

                case SolitaireKind.SPIDER:
                    return SpiderSettings.WinCount;
                default:
                    throw new Exception($"Solitaire not yet fully supported by UI: {solitaireKind}");
            }
        }

        public static Color GetWinsColor(int wins)
        {
            return Config.GetWinsColor(wins);
        }

        internal static string GetRules(SolitaireKind kind)
        {
            return Config.GetRules(kind);
        }
    }
}