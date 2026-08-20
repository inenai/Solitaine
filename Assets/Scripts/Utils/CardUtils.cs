using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Common;
using UnityEngine;

namespace Utils
{
    public static class CardUtils
    {
        public static string CardPrefabAddressText = "CARD_PREFAB_TEXT";
        public static string CardPrefabAddressSprite = "CARD_PREFAB";
        public const float CardStackZOffset = -0.1f;

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
            if (CardTextureGetter.Instance == null) throw new System.Exception("Card Texture Getter unavailable.");

            return CardTextureGetter.Instance.GetCardFace(card.Suit, card.Value);
        }

        public static Sprite GetCardBack()
        {
            if (CardTextureGetter.Instance == null) throw new System.Exception("Card Texture Getter unavailable.");

            return CardTextureGetter.Instance.GetBlueDeck();
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
    }
}