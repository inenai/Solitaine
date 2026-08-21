using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Common
{
    public class FreeCellView : TargetCardPileView
    {

        public override PileKind PileKind => PileKind.FREECELL;
        private CardView _cardView;
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

            _cardView = StackCardsInPosition(cards, _cardPosition, transform, "Card_FC_")[0];

            if (cardMoved != cards.Peek())
            {
                //Logs.Log($"TableauView[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            if (immediate)
            {
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(_cardView, originalCardPosition, _cardPosition, onDone);
        }
    }
}