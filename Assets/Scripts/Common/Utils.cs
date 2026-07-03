
using System;
using System.Collections.Generic;
using Model.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Common
{
    public static class Utils
    {

        public static string CardPrefabAddress = "CARD_PREFAB";

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

        public static void InstantiateAsync(string reference, Transform parent, Action<GameObject> onInstantiated, Action onError)
        {
            if (reference == null)
            {
                onError?.Invoke();
                return;
            }

            var op = Addressables.InstantiateAsync(reference,parent);
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

        public static bool SameCard(Card card, CardUI cardUI)
        {
            return
                card.Value == cardUI.Card.Value &&
                card.Suit == cardUI.Card.Suit;
        }
    }
}