
using System.Collections.Generic;

namespace Model.Common
{
    public class Foundation
    {
        public Stack<Card> Stack;
        public Enums.Suit Suit;

        public Foundation()
        {
            Stack = new();
            Suit = Enums.Suit.ANY;
        }
    }
}