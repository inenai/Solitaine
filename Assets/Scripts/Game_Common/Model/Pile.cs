using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

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

    public class CardPile : IEnumerable
    {
        private List<Card> _cards;

        public int Count => _cards.Count;

        public CardPile(IEnumerable<Card> collection)
        {
            _cards = new List<Card>(collection);
        }

        public CardPile() : base()
        {
            _cards = new();
        }

        public void Push(Card card)
        {
            _cards.Add(card);
        }

        public Card Pop()
        {
            if (_cards.Count == 0) throw new InvalidOperationException("CardPile is empty!");
            Card card = _cards[^1];
            _cards.Remove(card);
            return card;
        }

        public Card Peek()
        {
            if (_cards.Count == 0) throw new InvalidOperationException("CardPile is empty!");
            return _cards[^1];
        }

        public Card ElementAt(int index)
        {
            if (_cards.Count == 0) throw new InvalidOperationException("CardPile is empty!");
            int stackLikeIndex = _cards.Count - 1 - index;
            return _cards[stackLikeIndex];
        }

        public bool Contains(Card card)
        {
            return _cards.Contains(card);
        }

        public bool TryPeek(out Card topCard)
        {
            topCard = null;
            if (_cards.Count == 0) return false;
            topCard = Peek();
            return true;
        }

        public List<Card> Reverse()
        {
            List<Card> result = new List<Card>();
            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                result.Add(_cards[i]);
            }
            return result;
        }

        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                yield return _cards[i];
            }
        }

        public override string ToString()
        {
            return string.Join(" ", _cards);
        }
    }
}