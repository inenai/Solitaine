using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
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
            switch (kind)
            {
                case SolitaireKind.KLONDIKE:
                    return new Color(0f / 255f, 63f / 255f, 135f / 255f);
                case SolitaireKind.SAWAYAMA:
                    return new Color(70f / 255f, 5f / 255f, 14f / 255f);
                case SolitaireKind.FREECELL:
                    return new Color(9f / 255f, 113f / 255f, 82f / 255f);
                case SolitaireKind.SPIDER:
                    return new Color(183f / 255f, 140f / 255f, 16f / 255f);
                default:
                    return new Color(100f / 255f, 100f / 255f, 100f / 255f);
            }
        }

        public static Color GetUITextColor(SolitaireKind kind)
        {
            switch (kind)
            {
                case SolitaireKind.KLONDIKE:
                    return new Color(50f / 255f, 113f / 255f, 185f / 255f);
                case SolitaireKind.SAWAYAMA:
                    return new Color(120f / 255f, 55f / 255f, 64f / 255f);
                case SolitaireKind.FREECELL:
                    return new Color(59f / 255f, 163f / 255f, 132f / 255f);
                case SolitaireKind.SPIDER:
                    return new Color(255f / 255f, 190f / 255f, 0f / 255f);
                default:
                    return new Color(200f / 255f, 200f / 255f, 200f / 255f);
            }
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
            if (wins < 10) return new Color(1f, 1f, 1f);
            if (wins < 50) return new Color(0.7f, 0.4f, 0.23f);
            if (wins < 200) return new Color(0.67f, 0.67f, 0.67f);
            if (wins < 1000) return new Color(1f, 0.83f, 0f);
            if (wins < 5000) return new Color(0f, 1f, 0.67f);
            return new Color(1f, 0f, 0.66f);
        }
    }
}