using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;

namespace Klondike
{
    public class FoundationUI : TargetCardPileUI
    {
        [SerializeField] CardUI _cardUI_top;
        [SerializeField] CardUI _cardUI_bottom;

        private Vector3 _topPosition;
        private Vector3 _bottomPosition;

        void Awake()
        {
            _topPosition = _cardUI_top.transform.position;
            _bottomPosition = _cardUI_bottom.transform.position;
        }

        protected override void OnInit()
        {
            _cardUI_top.Init(_controller);
            _cardUI_bottom.Init(_controller);
        }

        public override Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            ResetCardPositions();
            if (cards.Count == 0)
            {
                _cardUI_top.gameObject.SetActive(false);
                _cardUI_bottom.gameObject.SetActive(false);
                return Task.CompletedTask;
            }

            if (cards.Count > 0)
            {
                _cardUI_top.gameObject.SetActive(cards.Count > 0);
                if (cards.Count > 0)
                {
                    _cardUI_top.LoadCardData(cards.Pop());
                }
                _cardUI_bottom.gameObject.SetActive(cards.Count > 0);
                if (cards.Count > 0)
                {
                    _cardUI_bottom.LoadCardData(cards.Pop());
                }
            }
            if (_cardUI_top.Card == cardMoved)
            {
                _cardUI_top.transform.position = originalCardPosition;
                return _cardUI_top.AnimateCard(_topPosition,0.1f);
            }
            else
                return Task.CompletedTask;
        }



        private void ResetCardPositions()
        {
            _cardUI_top.transform.position = _topPosition;
            _cardUI_bottom.transform.position = _bottomPosition;
        }
    }
}