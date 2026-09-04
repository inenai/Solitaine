using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Common
{
    public class TableauView : TargetCardPileView
    {
        public override PileKind PileKind => PileKind.TABLEAU;

        private float _offsetY = -0.5f;
        [SerializeField] private bool _dynamicYoffset = false;
        [SerializeField] private SpriteRenderer _verticalContainer;

        private List<CardView> _cardViews;
        private int _lastScreenSizeX;
        private int _lastScreenSizeY;
        private bool _dirty;

        protected override void OnInit()
        {
            _cardViews = new();
            _lastScreenSizeX = Screen.width;
            _lastScreenSizeY = Screen.height;
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

        void LateUpdate()
        {
            bool screenSizeChanged = _lastScreenSizeX != Screen.width || _lastScreenSizeY != Screen.height;
            _lastScreenSizeX = Screen.width;
            _lastScreenSizeY = Screen.height;

            if (!_dirty && !screenSizeChanged)
                return;

            if (!_dynamicYoffset ||
                _verticalContainer == null ||
                _cardViews == null ||
                _cardViews.Count <= 1)
            {
                return;
            }

            float cardHeight = _cardViews[0].Bounds.size.y;
            float containerHeight = _verticalContainer.bounds.size.y;

            // Height occupied by the stack using the normal offset.
            float stackHeight =
                cardHeight +
                (-_offsetY) * (_cardViews.Count - 1);

            if (stackHeight > containerHeight)
            {
                Layout();
            }
            else
            {
                RestoreNormalLayout();
            }
            _dirty = false;
        }

        private void UpdateCardView(
            int index,
            bool immediate,
            string cardGOName,
            float calculatedYOffset)
        {
            Transform desiredParent =
                index == 0
                    ? transform
                    : _cardViews[index - 1].transform;

            if (_cardViews[index].transform.parent != desiredParent)
            {
                _cardViews[index].transform.SetParent(desiredParent);
            }

            float yOffset = index == 0 ? 0f : calculatedYOffset;

            _cardViews[index].RefreshDynamicOffset(
                yOffset,
                CardUtils.CardStackZOffset);

            if (immediate)
                _cardViews[index].RefreshRevealedState();
            else
                _cardViews[index].PlayRevealIfNeeded();

            _cardViews[index].gameObject.name = cardGOName;
        }

        private void UpdateTriggerHightlight(float resultYOffset)
        {
            triggerHighlight.transform.position = new Vector3(
                triggerHighlight.transform.position.x,
                GetHighlightYPos(resultYOffset),
                -0.1f * (_cardViews.Count + 1));

            triggerHighlight.transform.localScale = new Vector3(
                triggerHighlight.transform.localScale.x,
                GetHighlightYScale(resultYOffset),
                1f);
        }

        public override void Refresh(
            CardPile tableauCards,
            Card cardMoved,
            Vector3 originalCardPosition,
            bool immediate,
            Action onDone,
            int displaceEvery = 0)
        {
            Card[] cards = ReloadCards(tableauCards);

            foreach (CardView cv in _cardViews)
            {
                cv.ShouldPlayLocked(true);
            }

            if (tableauCards.Count == 0)
            {
                UpdateTriggerHightlight(_offsetY);
                onDone?.Invoke();
                return;
            }

            _dirty = true;

            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;

            // Refresh always starts with the normal offset.
            float resultYOffset = _offsetY;

            for (int index = 0; index < cards.Length; index++)
            {
                bool isCardMoved = cards[index] == cardMoved;

                string cardGOName =
                    _cardViews[index].gameObject.name =
                        $"Card_T{Index}_{cards[index]}";

                UpdateCardView(
                    index,
                    immediate,
                    cardGOName,
                    resultYOffset);

                if (isCardMoved && !immediate)
                {
                    cardViewToAnimate = _cardViews[index];
                    cardViewToAnimateTargetPos =
                        _cardViews[index].transform.position;
                }
            }

            UpdateTriggerHightlight(resultYOffset);

            if (cardViewToAnimate == null)
            {
                onDone?.Invoke();
                return;
            }

            AnimateCardMoved(
                cardViewToAnimate,
                originalCardPosition,
                cardViewToAnimateTargetPos,
                onDone);
        }

        private float GetHighlightYPos(float resultYOffset)
        {
            if (_cardViews.Count <= 1)
            {
                return transform.position.y;
            }

            return transform.position.y +
                   resultYOffset * (_cardViews.Count - 1) / 2f;
        }

        private float GetHighlightYScale(float resultYOffset)
        {
            if (_cardViews.Count == 0)
                return _view.Deck.CardHeight;

            return _view.Deck.CardHeight +
                   (-resultYOffset) * (_cardViews.Count - 1);
        }

        private void Layout()
        {
            if (_cardViews.Count <= 1 ||
                _verticalContainer == null)
            {
                return;
            }

            Bounds containerBounds = _verticalContainer.bounds;

            float containerHeight = containerBounds.size.y;
            float cardHeight = _cardViews[0].Bounds.size.y;

            int cardCount = _cardViews.Count;

            // Calculate the spacing required for the cards to
            // fill the container from top to bottom.
            float spacing =
                (containerHeight - cardHeight) /
                (cardCount - 1);

            // First card = top of container.
            Vector3 position = _cardViews[0].transform.position;

            position.y =
                containerBounds.max.y -
                cardHeight * 0.5f;

            _cardViews[0].transform.position = position;

            // Every subsequent card is progressively lower.
            for (int i = 1; i < cardCount; i++)
            {
                Vector3 localPosition =
                    _cardViews[i].transform.localPosition;

                localPosition.y = -spacing;

                _cardViews[i].transform.localPosition =
                    localPosition;
            }

            UpdateTriggerHightlight(-spacing);
        }

        private void RestoreNormalLayout()
        {
            if (_cardViews.Count <= 1)
            {
                UpdateTriggerHightlight(_offsetY);
                return;
            }

            // Restore the normal spacing between cards.
            for (int i = 1; i < _cardViews.Count; i++)
            {
                Vector3 localPosition =
                    _cardViews[i].transform.localPosition;

                localPosition.y = _offsetY;

                _cardViews[i].transform.localPosition =
                    localPosition;
            }

            // Restore the first card to its normal position.
            Vector3 position = _cardViews[0].transform.position;
            position.y = transform.position.y;
            _cardViews[0].transform.position = position;

            UpdateTriggerHightlight(_offsetY);
        }
    }
}