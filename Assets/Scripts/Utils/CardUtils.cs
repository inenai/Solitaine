using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Common;
using Services;
using UnityEngine;

namespace Utils
{
    public static class CardUtils
    {
        public static string CardPrefabAddressText = "CARD_PREFAB_TEXT";
        public static string CardPrefabAddressSprite = "CARD_PREFAB";
        public const float CardStackZOffset = -0.05f;

        public const float MinCardStackYOffset = -0.2f;

        public static string GetSuitStr(CardSuit suit)
        {
            string suitStr = suit switch
            {
                CardSuit.HEARTS => "♥",
                CardSuit.DIAMONDS => "♦",
                CardSuit.CLUBS => "♣",
                CardSuit.SPADES => "♠",
                _ => "?"
            };
            return suitStr;
        }

        public static Color GetSuitColor(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.HEARTS:
                case CardSuit.DIAMONDS:
                    return Color.red;
                default:
                    return Color.black;
            }
        }

        public static bool SameCard(Card card, CardView cardUI)
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

        public static bool IsSameColor(CardSuit suit1, CardSuit suit2)
        {
            if (suit1 == suit2) return true;

            return (suit1 == CardSuit.DIAMONDS && suit2 == CardSuit.HEARTS) ||
                    (suit2 == CardSuit.DIAMONDS && suit1 == CardSuit.HEARTS) ||
                    (suit1 == CardSuit.CLUBS && suit2 == CardSuit.SPADES) ||
                    (suit2 == CardSuit.CLUBS && suit1 == CardSuit.SPADES);
        }

        public static Sprite GetCardSprite(Card card)
        {
            return God.Assets.Cards.GetCardFace(card.Suit, card.Value);
        }

        public static Sprite GetCardBack()
        {
            return God.Assets.Cards.GetBlueDeck();
        }

        public static List<CardPile> GetClonedCardPiles(CardPile[] stacks)
        {
            List<CardPile> result = new();
            foreach (CardPile s in stacks)
            {
                result.Add(CloneCardPile(s));
            }
            return result;
        }

        public static CardPile CloneCardPile(CardPile stack)
        {
            Contract.Requires(stack != null);
            return new CardPile(stack.Reverse());
        }

        public static CardSuit[] GetOppositeColorSuits(CardSuit suit)
        {
            if (suit == CardSuit.HEARTS || suit == CardSuit.DIAMONDS) return new CardSuit[] { CardSuit.SPADES, CardSuit.CLUBS };
            return new CardSuit[] { CardSuit.HEARTS, CardSuit.DIAMONDS };
        }

        public static void AddOneSetOfCardsToDeck(List<Card> deck, int suitsAmount = 4)
        {
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.SPADES, i));
            }
            CardSuit suit = suitsAmount > 1 ? CardSuit.HEARTS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
            suit = suitsAmount > 2 ? CardSuit.DIAMONDS : suitsAmount == 2 ? CardSuit.HEARTS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
            suit = suitsAmount > 2 ? CardSuit.CLUBS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
        }
    }
}