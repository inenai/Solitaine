using System.Collections.Generic;
using Utils;

namespace Common
{
    public class Foundation
    {
        public Stack<Card> Stack;
        public CardSuit? Suit => Stack.Count == 0? null : Stack.Peek().Suit;

        public Foundation()
        {
            Stack = new();
        }

        public override string ToString()
        {
            if (Suit == null) return $"F?";
            return $"F{CardUtils.GetSuitStr(Suit.Value)}";
        }
    }
}