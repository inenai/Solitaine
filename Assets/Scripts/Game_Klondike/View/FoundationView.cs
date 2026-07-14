using System.Collections.Generic;

using System.Threading.Tasks;
using Common;
using UnityEngine;
using Utils;

namespace Klondike
{
    public class FoundationView : TargetCardPileView
    {
        protected override void OnInit(){}

        public override async Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            Debug.Log($"Foundation[{Index}] refreshing...");
            if (cards.Count == 0)
            {
                Debug.Log($"Foundation[{Index}] refreshed.");
                return;
            }

            await StackCardsInPositionWithAnimation(cards, transform.position, transform, $"Card_F{Index}_", cardMoved, originalCardPosition);
            Debug.Log($"Foundation[{Index}] refreshed.");
        }
    }
}