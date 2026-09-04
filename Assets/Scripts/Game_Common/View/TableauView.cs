using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Common
{
    public class TableauView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.TABLEAU;
        [SerializeField] private float _offsetY = -0.3f;
        [SerializeField] private bool _dynamicYoffset = false;
        int _dynamicYoffsetFrom = 10;

        private List<CardView> _cardViews;

        protected override void OnInit()
        {
            _cardViews = new();
        }

        private Card[] ReloadCards(CardPile tableauCards)
        {
            _cardViews.Clear();
            Card[] cards = tableauCards.Reverse().ToArray();
            foreach (Card card in cards)
            {
                _cardViews.Add(_view.Deck.GetCardView(card));
            }
            return cards;
        }

        private void UpdateCardView(int index, bool immediate, string cardGOName, float calculatedYOffset)
        {
            Transform desiredParent = index == 0 ? transform : _cardViews[index - 1].transform;
            if (_cardViews[index].transform.parent != desiredParent)
            {
                _cardViews[index].transform.SetParent(desiredParent);
            }
            float yOffset = index == 0 ? 0f : calculatedYOffset;
            _cardViews[index].RefreshDynamicOffset(yOffset, CardUtils.CardStackZOffset);
            if (immediate)
                _cardViews[index].RefreshRevealedState();
            else
                _cardViews[index].PlayRevealIfNeeded();
            _cardViews[index].gameObject.name = cardGOName;
        }

        private void UpdateTriggerHightlight(float resultYOffset)
        {
            triggerHighlight.transform.position = new Vector3(triggerHighlight.transform.position.x, GetHighlightYPos(resultYOffset), -0.1f * (_cardViews.Count + 1));
            triggerHighlight.transform.localScale = new Vector3(triggerHighlight.transform.localScale.x, GetHighlightYScale(resultYOffset), 1f);
        }

        public override void Refresh(CardPile tableauCards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone, int displaceEvery = 0)
        {
            //Logs.Log($"TableauView[{Index}] refreshing...");

            Card[] cards = ReloadCards(tableauCards);

            foreach (CardView cv in _cardViews)
            {
                cv.ShouldPlayLocked(true);
            }

            if (tableauCards.Count == 0)
            {
                //Logs.Log($"TableauView[{Index}] refreshed.");
                UpdateTriggerHightlight(_offsetY);
                onDone?.Invoke();
                return;
            }

            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;

            float resultYOffset = _offsetY;
            if (_dynamicYoffset)
            {
                if (tableauCards.Count > _dynamicYoffsetFrom)
                {
                    float extraCards = tableauCards.Count - _dynamicYoffsetFrom;
                    float t = 1f / (extraCards + 1f);
                    resultYOffset = Mathf.Lerp(CardUtils.MinCardStackYOffset, _offsetY, t);
                }
            }

            for (int index = 0; index < cards.Length; index++)
            {
                bool isCardMoved = cards[index] == cardMoved;
                string cardGOName = _cardViews[index].gameObject.name = $"Card_T{Index}_{cards[index]}";

                UpdateCardView(index, immediate, cardGOName, resultYOffset);
                if (isCardMoved && !immediate)
                {
                    cardViewToAnimate = _cardViews[index];
                    cardViewToAnimateTargetPos = _cardViews[index].transform.position;
                }
            }

            UpdateTriggerHightlight(resultYOffset);

            if (cardViewToAnimate == null)
            {
                //Logs.Log($"TableauView[{Index}] refreshed.");
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(cardViewToAnimate, originalCardPosition, cardViewToAnimateTargetPos, onDone);
        }

        private float GetHighlightYPos(float resultYOffset)
        {
            if (_cardViews.Count <= 1)
            {
                return transform.position.y;
            }

            return transform.position.y + resultYOffset * (_cardViews.Count - 1) / 2f;
        }

        private float GetHighlightYScale(float resultYOffset)
        {
            if (_cardViews.Count == 0) return _view.Deck.CardHeight;
            return _view.Deck.CardHeight + (-resultYOffset) * (_cardViews.Count - 1);
        }
    }
}