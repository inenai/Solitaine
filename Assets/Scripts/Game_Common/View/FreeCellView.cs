using System;
using UnityEngine;
using Utils;

namespace Common
{
    public class FreeCellView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.FREECELL;
        private Vector3 _cardPosition;

        protected override void OnInit()
        {
            _cardPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + CardUtils.CardStackZOffset);
        }

        public override void Refresh(CardPile cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone, int displaceEvery = 0)
        {
            if (cards.Count > 1) throw new Exception($"A FreeCell can't contain more than one card. Tried to load {cards.Count}");
            if (cards.Count == 0)
            {
                onDone?.Invoke();
                //Logs.Log($"FreeCellView[{Index}] refreshed.");
                return;
            }

            Card card = cards.ElementAt(0);

            if (immediate || cardMoved != card)
            {
                StackCardsInPosition(cards, _cardPosition, transform, "Card_FC_");
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(_view.Deck.GetCardView(card), originalCardPosition, _cardPosition, transform, onDone);
        }
    }
}