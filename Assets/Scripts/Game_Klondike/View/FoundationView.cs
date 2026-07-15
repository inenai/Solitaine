using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    public class FoundationView : TargetCardPileView
    {
        protected override void OnInit(){}

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, Action onDone)
        {
            Debug.Log($"Foundation[{Index}] refreshing...");
            if (cards.Count == 0)
            {
                Debug.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            StackCardsInPositionWithAnimation(cards, transform.position, transform, $"Card_F{Index}_", cardMoved, originalCardPosition, () =>
            {
                Debug.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
            });
        }
    }
}