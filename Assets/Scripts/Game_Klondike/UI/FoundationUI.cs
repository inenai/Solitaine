using System;
using System.Collections.Generic;
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

        public void Refresh(Stack<Card> stock, Action onDone)
        {
            if (stock.Count == 0)
            {
                _cardUI_top.gameObject.SetActive(false);
                _cardUI_bottom.gameObject.SetActive(false);
                onDone?.Invoke();
                return;
            }

            if (stock.Count > 0)
            {
                _cardUI_top.gameObject.SetActive(stock.Count > 0);
                if (stock.Count > 0)
                {
                    _cardUI_top.LoadCardData(stock.Pop());
                }
                _cardUI_bottom.gameObject.SetActive(stock.Count > 0);
                if (stock.Count > 0)
                {
                    _cardUI_bottom.LoadCardData(stock.Pop());
                }
            }
            onDone?.Invoke();
        }
    }
}