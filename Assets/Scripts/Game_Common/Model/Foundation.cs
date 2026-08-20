using Utils;

namespace Common
{
    public class Foundation : CardPile
    {
        public CardSuit? Suit => Count == 0? null : Peek().Suit;

        public override string ToString()
        {
            if (Suit == null) return $"F?";
            return $"F{CardUtils.GetSuitStr(Suit.Value)}";
        }
    }
}