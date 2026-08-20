using System.Collections.Generic;

namespace Common {
    public struct PileData
    {
        public int Index;
        public PileKind Kind;

        public PileData(PileKind kind, int index) : this()
        {
            Kind = kind;
            Index = index;
        }
    }

    public class CardPile : Stack<Card>
    {
        public new int Count => base.Count;

        public CardPile(IEnumerable<Card> collection) : base(collection)
        {
        }

        public CardPile() : base()
        {
        }

        protected virtual void InnerPush(Card card)
        {
            base.Push(card);
        }

        protected virtual Card InnerPop()
        {
            return base.Pop();
        }

        public new void Push(Card card)
        {
            InnerPush(card);
        }

        public new Card Pop()
        {
            return InnerPop();
        }
    }
}