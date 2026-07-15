using System;
using System.Collections.Generic;
using System.Linq;
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


        private Card[] ReloadCards(Stack<Card> tableauCards)
        {
            _cardViews.Clear();
            Card[] cards = tableauCards.Reverse().ToArray();
            foreach (Card card in cards)
            {
                _cardViews.Add(_view.Deck.GetCardView(card));
            }
            return cards;
        }

        private void UpdateCardView(int index, bool initRefresh, string cardGOName)
        {
            Transform desiredParent = index == 0 ? transform : _cardViews[index - 1].transform;
            if (_cardViews[index].transform.parent != desiredParent)
            {
                _cardViews[index].transform.SetParent(desiredParent);
            }
            float yOffset = index == 0 ? 0f : _offsetY;
            _cardViews[index].RefreshDynamicOffset(yOffset, _offsetZ);
            if (initRefresh)
                _cardViews[index].UpdateRevealedState();
            else
                _cardViews[index].PlayRevealIfNeeded();
            _cardViews[index].gameObject.name = cardGOName;
        }

        private void UpdateTriggerHightlight()
        {
            triggerHighlight.transform.position = new Vector3(triggerHighlight.transform.position.x, GetHighlightYPos(), -0.1f * (_cardViews.Count + 1));
            triggerHighlight.transform.localScale = new Vector3(triggerHighlight.transform.localScale.x, GetHighlightYScale(), 1f);
        }

        private void AnimateCardMoved(CardView cardViewToAnimate, Vector3 originalCardPosition, Vector3 cardViewToAnimateTargetPos, Action onDone)
        {
            cardViewToAnimate.transform.position = originalCardPosition;
            cardViewToAnimate.AnimateCard(cardViewToAnimateTargetPos, CardView.DefaultCardFlyTime, onDone);
        }

        public override void Refresh(Stack<Card> tableauCards, Card cardMoved, Vector3 originalCardPosition, bool initRefresh, Action onDone)
        {
            Debug.Log($"TableauView[{Index}] refreshing...");

            Card[] cards = ReloadCards(tableauCards);

            if (tableauCards.Count == 0)
            {
                Debug.Log($"TableauView[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;

            for (int index = 0; index < cards.Length; index++)
            {
                bool isCardMoved = cards[index] == cardMoved;
                string cardGOName = _cardViews[index].gameObject.name = $"Card_T{Index}_{cards[index]}";
                UpdateCardView(index, initRefresh, cardGOName);
                if (isCardMoved)
                {
                    cardViewToAnimate = _cardViews[index];
                    cardViewToAnimateTargetPos = _cardViews[index].transform.position;
                }
            }

            UpdateTriggerHightlight();

            if (cardViewToAnimate == null)
            {
                Debug.Log($"TableauView[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(cardViewToAnimate, originalCardPosition, cardViewToAnimateTargetPos, onDone);
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