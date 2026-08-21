using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Common
{
    public abstract class CardPileView : MonoBehaviour
    {
        [SerializeField] private float _xStackOffset = 0.12f;
        public abstract PileKind PileKind { get; }
        public int Index => _index;

        protected int _index = -1;
        protected GameView _view;

        public abstract void Refresh(CardPile cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone, int displaceEvery = 0);

        public void Init(GameView gameView, int index = -1)
        {
            _view = gameView;
            _index = index;
            OnInit();
        }

        protected abstract void OnInit();

        protected List<CardView> StackCardsInPositionWithAnimation(CardPile cards, Vector3 position, Transform parent, string cardPrefix, Card cardMoved, Vector3 originalCardPosition, Action onDone, int displaceEvery = 0)
        {
            float zOffset = CardUtils.CardStackZOffset;
            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;
            List<CardView> result = new();
            int counter = displaceEvery;

            foreach (Card card in cards.Reverse())
            {
                CardView cv = _view.Deck.GetCardView(card);
                cv.transform.SetParent(parent);
                cv.transform.position = new Vector3(position.x, position.y, position.z + zOffset);

                if (displaceEvery > 0)
                {
                    if (counter == 0)
                    {
                        cv.transform.position = new Vector3(cv.transform.position.x + _xStackOffset, cv.transform.position.y, cv.transform.position.z);
                        counter = displaceEvery;
                    }
                    counter--;
                }

                zOffset += CardUtils.CardStackZOffset;
                cv.gameObject.name = $"{cardPrefix}{card}";
                cv.PlayRevealIfNeeded();
                result.Add(cv);
                if (card == cardMoved)
                {
                    cardViewToAnimate = cv;
                    cardViewToAnimateTargetPos = cv.transform.position;
                }
            }

            if (cardViewToAnimate == null)
            {
                onDone?.Invoke();
                return result;
            }

            AnimateCardMoved(cardViewToAnimate, originalCardPosition, cardViewToAnimateTargetPos, onDone);
            return result;
        }

        protected void AnimateCardMoved(CardView cardViewToAnimate, Vector3 originalCardPosition, Vector3 cardViewToAnimateTargetPos, Action onDone)
        {
            cardViewToAnimate.transform.position = originalCardPosition;
            cardViewToAnimate.AnimateCard(cardViewToAnimateTargetPos, CardView.GetFlightTime(originalCardPosition, cardViewToAnimateTargetPos), onDone);
        }

        protected List<CardView> StackCardsInPosition(CardPile cards, Vector3 position, Transform parent, string cardPrefix, int displaceEvery = 0)
        {
            float zOffset = CardUtils.CardStackZOffset;
            List<CardView> result = new();
            int counter = displaceEvery;
            float displaceMult = 0f;

            foreach (Card card in cards.Reverse())
            {
                CardView cv = _view.Deck.GetCardView(card);
                cv.transform.SetParent(parent);
                cv.transform.position = new Vector3(position.x + displaceMult, position.y, position.z + zOffset);

                if (displaceEvery > 0)
                {
                    if (counter == 0)
                    {
                        displaceMult += _xStackOffset;
                        counter = displaceEvery;
                    }
                    counter--;
                }

                zOffset += CardUtils.CardStackZOffset;
                cv.gameObject.name = $"{cardPrefix}{card}";
                cv.RefreshRevealedState();
                result.Add(cv);
            }
            return result;
        }
    }
}