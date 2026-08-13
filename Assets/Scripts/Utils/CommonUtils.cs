using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Common;
using FreeCell;
using Klondike;
using Sawayama;
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

        public static Stack<T> CloneStack<T>(Stack<T> stack)
        {
            Contract.Requires(stack != null);
            return new Stack<T>(stack.Reverse());
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
                default:
                    throw new Exception($"Solitaire not yet fully supported by UI: {solitaireKind}");
            }
        }
    }
}