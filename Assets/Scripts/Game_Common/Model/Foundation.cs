using System.Collections.Generic;

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
    }
}