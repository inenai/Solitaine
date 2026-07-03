using System;
using System.Collections.Generic;
using System.Linq;
using Model.Common;
using UnityEngine;

public class WasteUI : CardPileUI
{
    [SerializeField] CardUI[] _cardUIs;

    private List<Card> _wasteCards;

    public void Refresh(Stack<Card> waste, Action onDone)
    {
        if (_cardUIs.Length < 1)
        {
            throw new Exception("Waste has no card UI available in scene!");
        }

        _wasteCards = waste.ToList();

        RefreshWasteCardUIs();
        TurnOffUnusedCardUIs();

        onDone?.Invoke();
    }

    private void RefreshWasteCardUIs()
    {
        for (int i = _wasteCards.Count - 1; i >= 0; i--)
        {
            if (_cardUIs.Length > i)
            {
                CardUI cardUI = _cardUIs[i];
                Debug.Log($"Activating waste card index {i}.");
                cardUI.gameObject.SetActive(true);
                cardUI.Init(_wasteCards[i], _controller);
            }
        }
    }

    private void TurnOffUnusedCardUIs()
    {
        if (_wasteCards.Count < _cardUIs.Length)
        {
            for (int i = _wasteCards.Count; i < _cardUIs.Length; i++)
            {
                Debug.Log($"Deactivating waste card index {i}.");
                _cardUIs[i].gameObject.SetActive(false);
            }
        }
    }
}
