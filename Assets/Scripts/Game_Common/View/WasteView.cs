using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Common
{
    public class WasteView : CardPileView
    {
        public override PileKind PileKind => PileKind.WASTE;

        [SerializeField] private int _maxCardsInView = 1;
        [SerializeField] private float _xOffset = 0.3f;
        protected List<CardView> _cardViews;

        protected override void OnInit() { }
        private Vector3 GetPosition(int cardIndex, int slotIndex)
        {
            Vector3 position = new Vector3(
                transform.position.x + _xOffset * slotIndex,
                transform.position.y,
                transform.position.z + CardUtils.CardStackZOffset * (cardIndex + 1));
            return position;
        }

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            //Debug.Log("WasteView refreshing...");
            if (cards.Count == 0)
            {
                //Debug.Log("WasteView refreshed.");
                onDone?.Invoke();
                return;
            }

            _cardViews = StackCardsInPosition(cards, GetPosition(0,0), transform, "Card_W_");

            int amount = _cardViews.Count;
            for (int i = amount - 1; i >= amount - _maxCardsInView; i--)
            {
                if (i < 0) break;
                int cardSlotIndex;
                if (amount > _maxCardsInView)
                {
                    cardSlotIndex = i - (amount - _maxCardsInView);
                }
                else
                {
                    cardSlotIndex = i;
                }

                _cardViews[i].transform.position = GetPosition(i, cardSlotIndex);

                Debug.Log($"Card {_cardViews[i].Card} index {i} to card slot {cardSlotIndex}");
            }

            //Debug.Log("WasteView refreshed.");
            onDone?.Invoke();
        }
    }
}