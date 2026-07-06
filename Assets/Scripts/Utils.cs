using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Model.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static Model.Common.Enums;

namespace Common.Utils
{
    public static class Utils
    {

        public static string CardPrefabAddress = "CARD_PREFAB";

        #region General
        public static IList<T> Shuffle<T>(IList<T> list)
        {
            System.Random rng = new System.Random(1234);
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

        public static Stack<T> Clone<T>(this Stack<T> stack)
        {
            Contract.Ensures(Contract.Result<Stack<T>>() != null);
                if (stack == null) return new Stack<T>();
            return new Stack<T>(stack.Reverse());
        }
        #endregion

        #region Cards
        public static string GetSuitStr(Enums.Suit suit)
        {
            string suitStr = suit switch
            {
                Enums.Suit.HEARTS => "♥",
                Enums.Suit.DIAMONDS => "♦",
                Enums.Suit.CLUBS => "♣",
                Enums.Suit.SPADES => "♠",
                _ => "?"
            };
            return suitStr;
        }

        public static Color GetSuitColor(Enums.Suit suit)
        {
            switch (suit)
            {
                case Enums.Suit.HEARTS:
                case Enums.Suit.DIAMONDS:
                    return Color.red;
                default:
                    return Color.black;
            }
        }

        public static bool SameCard(Card card, CardUI cardUI)
        {
            return
                card.Value == cardUI.Card.Value &&
                card.Suit == cardUI.Card.Suit;
        }

        public static string CardValue(Card card)
        {
            string value = card.Value.ToString();
            switch (card.Value)
            {
                case 11:
                    value = "J";
                    break;
                case 12:
                    value = "Q";
                    break;
                case 13:
                    value = "K";
                    break;
            }
            return value;
        }

        public static string CardToShortString(Card card)
        {
            if (card == null) return "null";

            string suit = Utils.GetSuitStr(card.Suit);

            string revealed = "";
            if (card.Revealed)
            {
                revealed = "*";
            }
            return $"{CardValue(card)}{suit}{revealed}";
        }

        public static bool IsSameColor(Suit suit1, Suit suit2)
        {
            if (suit1 == suit2) return true;

            return (suit1 == Suit.DIAMONDS && suit2 == Suit.HEARTS) ||
                    (suit2 == Suit.DIAMONDS && suit1 == Suit.HEARTS) ||
                    (suit1 == Suit.CLUBS && suit2 == Suit.SPADES) ||
                    (suit2 == Suit.CLUBS && suit1 == Suit.SPADES);
        }

        #endregion

        #region AssetManagement
        public static void InstantiateAsync(string reference, Transform parent, Action<GameObject> onInstantiated, Action onError)
        {
            if (reference == null)
            {
                onError?.Invoke();
                return;
            }

            var op = Addressables.InstantiateAsync(reference, parent);
            op.Completed += (opHandle) =>
            {
                if (opHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    onInstantiated?.Invoke(opHandle.Result);
                }
                else
                {
                    Debug.LogError($"Failed to load asset at {reference}. Status: {op.Status}");
                    onError?.Invoke();
                }
            };
        }
#endregion

    }
}

    public struct CardPileData
    {
        public int Index;
        public PileKind Kind;

        public CardPileData(PileKind kind, int index) : this()
        {
            Kind = kind;
            Index = index;
        }
    }

