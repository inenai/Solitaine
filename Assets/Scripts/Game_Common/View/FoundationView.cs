using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class FoundationView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.FOUNDATION;

        protected override void OnInit(){}

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            Debug.Log($"Foundation[{Index}] refreshing...");
            if (cards.Count == 0)
            {
                Debug.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            if (immediate)
            {
                StackCardsInPosition(cards, transform.position, transform, $"Card_F{Index}_");
                Debug.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
            }
            else
            {
                StackCardsInPositionWithAnimation(cards, transform.position, transform, $"Card_F{Index}_", cardMoved, originalCardPosition, () =>
                {
                    Debug.Log($"Foundation[{Index}] refreshed.");
                    onDone?.Invoke();
                });
            }
        }
    }
}