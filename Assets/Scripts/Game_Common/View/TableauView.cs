using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Common
{
    public class TableauView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.TABLEAU;
        private const float _offsetY = -0.3f;

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

        private void UpdateCardView(int index, bool immediate, string cardGOName)
        {
            Transform desiredParent = index == 0 ? transform : _cardViews[index - 1].transform;
            if (_cardViews[index].transform.parent != desiredParent)
            {
                _cardViews[index].transform.SetParent(desiredParent);
            }
            float yOffset = index == 0 ? 0f : _offsetY;
            _cardViews[index].RefreshDynamicOffset(yOffset, CardUtils.CardStackZOffset);
            if (immediate)
                _cardViews[index].RefreshRevealedState();
            else
                _cardViews[index].PlayRevealIfNeeded();
            _cardViews[index].gameObject.name = cardGOName;
        }

        private void UpdateTriggerHightlight()
        {
            triggerHighlight.transform.position = new Vector3(triggerHighlight.transform.position.x, GetHighlightYPos(), -0.1f * (_cardViews.Count + 1));
            triggerHighlight.transform.localScale = new Vector3(triggerHighlight.transform.localScale.x, GetHighlightYScale(), 1f);
        }

        public override void Refresh(Stack<Card> tableauCards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            //Debug.Log($"TableauView[{Index}] refreshing...");

            Card[] cards = ReloadCards(tableauCards);

            if (tableauCards.Count == 0)
            {
                //Debug.Log($"TableauView[{Index}] refreshed.");
                UpdateTriggerHightlight();
                onDone?.Invoke();
                return;
            }

            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;

            for (int index = 0; index < cards.Length; index++)
            {
                bool isCardMoved = cards[index] == cardMoved;
                string cardGOName = _cardViews[index].gameObject.name = $"Card_T{Index}_{cards[index]}";
                UpdateCardView(index, immediate, cardGOName);
                if (isCardMoved && !immediate)
                {
                    cardViewToAnimate = _cardViews[index];
                    cardViewToAnimateTargetPos = _cardViews[index].transform.position;
                }
            }

            UpdateTriggerHightlight();

            if (cardViewToAnimate == null)
            {
                //Debug.Log($"TableauView[{Index}] refreshed.");
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