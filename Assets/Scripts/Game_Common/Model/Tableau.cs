using System.Collections.Generic;

namespace Common
{
    public class Tableau : CardPile
    {
        public List<Card> ListCopy => new List<Card>(_list);
        private List<Card> _list;

        public Tableau() : base()
        {
            _list = new();
        }

        protected override void InnerPush(Card card)
        {
            _list.Add(card);
            base.InnerPush(card);
        }

        protected override Card InnerPop()
        {
            if (_list.Count > 0)
                _list.RemoveAt(_list.Count-1);

            return base.InnerPop();
        }
    }
}