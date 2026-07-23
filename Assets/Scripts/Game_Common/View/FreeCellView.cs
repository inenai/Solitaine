using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class FreeCellView : TargetCardPileView
    {

        private CardView _cardView;

        protected override void OnInit() { }

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            if (cards.Count > 1) throw new Exception($"A FreeCell can't contain more than one card. Tried to load {cards.Count}");
            if (cards.Count == 0)
            {
                onDone?.Invoke();
                Debug.Log($"FreeCellView[{Index}] refreshed.");
                return;
            }

            _cardView = StackCardsInPosition(cards, transform.position, transform, "Card_FC_")[0];

            if (cardMoved != cards.Peek())
            {
                Debug.Log($"TableauView[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(_cardView, originalCardPosition, transform.position, onDone);
        }


    }
}