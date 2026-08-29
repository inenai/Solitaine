using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class FoundationView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.FOUNDATION;

        protected override void OnInit(){}

        public override void Refresh(CardPile cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone, int displaceEvery = 0)
        {
            //Logs.Log($"Foundation[{Index}] refreshing...");
            if (cards.Count == 0)
            {
                //Logs.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            if (immediate)
            {
                List<CardView> cardViews = StackCardsInPosition(cards, transform.position, transform, $"Card_F{Index}_");
                foreach (CardView cv in cardViews)
                {
                    cv.ShouldPlayLocked(true);
                }
                //Logs.Log($"Foundation[{Index}] refreshed.");
                onDone?.Invoke();
            }
            else
            {
                StackCardsInPositionWithAnimation(cards, transform.position, transform, $"Card_F{Index}_", cardMoved, originalCardPosition, () =>
                {
                    //Logs.Log($"Foundation[{Index}] refreshed.");
                    onDone?.Invoke();
                });
            }
        }
    }
}