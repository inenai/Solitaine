using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public abstract Task Refresh(Stack<Card> cards,Card cardMoved, Vector3 originalCardPosition);

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
                cv.UpdateRevealed();
                result.Add(cv);
            }
            return result;
        }

        protected Task StackCardsInPositionWithAnimation(Stack<Card> cards, Vector3 position, Transform parent, string cardPrefix, Card cardMoved, Vector3 originalCardPosition)
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
                cv.UpdateRevealed();
                if (card == cardMoved)
                {
                    cardViewToAnimate = cv;
                    cardViewToAnimateTargetPos = cv.transform.position;
                }
            }

            if (cardViewToAnimate == null)
            {
                return Task.CompletedTask;
            }

            cardViewToAnimate.transform.position = originalCardPosition;
            return cardViewToAnimate.AnimateCard(cardViewToAnimateTargetPos, CardView.DefaultCardFlyTime);
        }
    }
}