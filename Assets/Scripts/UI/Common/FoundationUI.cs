using System;
using System.Collections.Generic;
using Model.Common;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FoundationUI : CardPileUI
{
    [SerializeField] CardUI _cardUI_top;
    [SerializeField] CardUI _cardUI_bottom;

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
                _cardUI_top.Init(stock.Pop(), _controller);
            }

            if (stock.Count > 0)
            {
                _cardUI_bottom.gameObject.SetActive(stock.Count > 0);
                if (stock.Count > 0)
                {
                    _cardUI_bottom.Init(stock.Pop(), _controller);
                }
            }
        }
        onDone?.Invoke();
    }
}
