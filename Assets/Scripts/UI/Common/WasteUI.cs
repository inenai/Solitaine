using System;
using System.Collections.Generic;
using System.Linq;
using Model.Common;
using UnityEngine;

public class WasteUI : CardPileUI
{
    /// <summary>
    /// <para> index => gameobject</para>
    /// <para>0 => 1st card, leftmost</para>
    /// <para>1 => 2nd card, middle</para>
    /// <para>2 => 3rd card, rightmost</para>
    /// </summary>
    [SerializeField] CardUI[] _cardUIs;

    public void Refresh(Stack<Card> waste, Action onDone)
    {
        if (_cardUIs.Length < 1)
        {
            throw new Exception("Waste has no card UI available in scene!");
        }

        TurnOffUnusedCardUIs(waste.Count);
        RefreshWasteCardUIs(waste);

        onDone?.Invoke();
    }

    private void RefreshWasteCardUIs(Stack<Card> waste)
    {
        int j = waste.Count > 2 ? 2 : waste.Count - 1;
        while (waste.Count > 0 && j >= 0)
        {
            CardUI cardUI = _cardUIs[j];
            Debug.Log($"Activating waste card index {j}.");
            cardUI.gameObject.SetActive(true);
            cardUI.Init(waste.Pop(), _controller);
            j--;
        }
    }

    private void TurnOffUnusedCardUIs(int count)
    {
        if (count < _cardUIs.Length)
        {
            for (int i = count; i < _cardUIs.Length; i++)
            {
                //Debug.Log($"Deactivating waste card index {i}.");
                _cardUIs[i].gameObject.SetActive(false);
            }
        }
    }
}
