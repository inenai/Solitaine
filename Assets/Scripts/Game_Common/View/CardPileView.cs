using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Common
{
    public abstract class CardPileView : MonoBehaviour
    {

        [SerializeField] protected PileKind _pileKind;

        public PileKind PileKind => _pileKind;
        public int Index => _index;

        protected int _index = -1;
        protected GameView _view;

        public abstract void Refresh(Stack<Card> cards,Card cardMoved, Vector3 originalCardPosition, Action onDone);

        public void Init(GameView gameView, int index = -1)
        {
            _view = gameView;
            _index = index;
            OnInit();
        }

        protected abstract void OnInit();

        protected List<CardView> StackCardsInPosition(Stack<Card> cards, Vector3 position, Transform parent, string cardPrefix)
        {
            float zOffset = CardUtils.CardStackZOffset;
            List<CardView> result = new();

            foreach (Card card in cards.Reverse())
            {
                CardView cv = _view.Deck.GetCardView(card);
                cv.transform.SetParent(parent);
                cv.transform.position = new Vector3(position.x, position.y, position.z + zOffset);
                zOffset += CardUtils.CardStackZOffset;
                cv.gameObject.name = $"{cardPrefix}{card}";
                cv.UpdateRevealedState();
                result.Add(cv);
            }
            return result;
        }

        protected void StackCardsInPositionWithAnimation(Stack<Card> cards, Vector3 position, Transform parent, string cardPrefix, Card cardMoved, Vector3 originalCardPosition, Action onDone)
        {
            float zOffset = CardUtils.CardStackZOffset;
            CardView cardViewToAnimate = null;
            Vector3 cardViewToAnimateTargetPos = default;
            foreach (Card card in cards.Reverse())
            {
                CardView cv = _view.Deck.GetCardView(card);
                cv.transform.SetParent(parent);
                cv.transform.position = new Vector3(position.x, position.y, position.z + zOffset);
                zOffset += CardUtils.CardStackZOffset;
                cv.gameObject.name = $"{cardPrefix}{card}";
                cv.PlayRevealIfNeeded();
                if (card == cardMoved)
                {
                    cardViewToAnimate = cv;
                    cardViewToAnimateTargetPos = cv.transform.position;
                }
            }

            if (cardViewToAnimate == null)
            {
                onDone?.Invoke();
                return;
            }

            cardViewToAnimate.transform.position = originalCardPosition;
            cardViewToAnimate.AnimateCard(cardViewToAnimateTargetPos, CardView.DefaultCardFlyTime, onDone);
        }
    }
}