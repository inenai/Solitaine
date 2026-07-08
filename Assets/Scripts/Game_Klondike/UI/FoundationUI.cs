using System;
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

        protected override void OnInit()
        {
            _cardUI_top.Init(_controller);
            _cardUI_bottom.Init(_controller);
        }

        public override Task Refresh(Stack<Card> cards)
        {
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
            return Task.CompletedTask;
        }
    }
}