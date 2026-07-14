using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using UnityEngine;

namespace Klondike
{
    public class TableauView : TargetCardPileView
    {
        private const float _offsetY = -0.3f;
        private const float _offsetZ = -0.1f;

        private List<CardView> _cardViews;

        protected override void OnInit()
        {
            _cardViews = new();
        }

        public override Task Refresh(Stack<Card> tableauCards, Card cardMoved, Vector3 originalCardPosition)
        {
            Debug.Log($"TableauView[{Index}] refreshing...");
            _cardViews.Clear();

            if (tableauCards.Count == 0)
            {
                Debug.Log($"TableauView[{Index}] refreshed.");
                return Task.CompletedTask;
            }

            Card[] cards = tableauCards.Reverse().ToArray();
            foreach (Card card in cards)
            {
                _cardViews.Add(_view.Deck.GetCardView(card));
            }

            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;

            for (int i = 0; i < cards.Length; i++)
            {
                Transform desiredParent = i == 0 ? transform : _cardViews[i - 1].transform;
                if (_cardViews[i].transform.parent != desiredParent)
                {
                    _cardViews[i].transform.SetParent(desiredParent);
                }
                float yOffset = i == 0 ? 0f : _offsetY;
                _cardViews[i].LoadCardData(cards[i], yOffset, _offsetZ);
                _cardViews[i].gameObject.name = $"Card_T{Index}_{cards[i]}";
                if (cards[i] == cardMoved)
                {
                    cardViewToAnimate = _cardViews[i];
                    cardViewToAnimateTargetPos = _cardViews[i].transform.position;
                }
            }

            triggerHighlight.transform.position = new Vector3(triggerHighlight.transform.position.x, GetHighlightYPos(), -0.1f * (_cardViews.Count + 1));
            triggerHighlight.transform.localScale = new Vector3(triggerHighlight.transform.localScale.x, GetHighlightYScale(), 1f);

            if (cardViewToAnimate == null)
            {
                Debug.Log($"TableauView[{Index}] refreshed.");
                return Task.CompletedTask;
            }

            cardViewToAnimate.transform.position = originalCardPosition;
            return cardViewToAnimate.AnimateCard(cardViewToAnimateTargetPos, CardView.DefaultCardFlyTime);
        }

        private float GetHighlightYPos()
        {
            if (_cardViews.Count <= 1)
            {
                return transform.position.y;
            }

            return transform.position.y + _offsetY * (_cardViews.Count - 1) / 2f;
        }

        private float GetHighlightYScale()
        {
            if (_cardViews.Count == 0) return _view.Deck.CardHeight;
            return _view.Deck.CardHeight + (-_offsetY) * (_cardViews.Count - 1);
        }
    }
}