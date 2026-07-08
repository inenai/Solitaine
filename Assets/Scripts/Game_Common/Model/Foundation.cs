using System.Collections.Generic;
using Utils;

namespace Common
{
    public class Foundation
    {
        public Stack<Card> Stack;
        public CardSuit Suit;

        public Foundation()
        {
            Stack = new();
            Suit = CardSuit.ANY;
        }

        public override string ToString()
        {
            if (Suit == CardSuit.ANY) return $"F";
            return $"F{CardUtils.GetSuitStr(Suit)}";
        }
    }
}