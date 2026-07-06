using System;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;
using Model.Common;
using UI.Common;
using UnityEngine;

public class TableauUI : CardPileUI
{
    private float _offsetY = -0.3f;
    private float _offsetZ = 0.1f;
    private List<CardUI> _cardUIs;

    public void Init(IGameController controller, int index, Stack<Card> cards, Action onDone = null)
    {
        //Debug.Log($"Init tableau [{index}] with {cards.Count} cards");
        Init(controller, index);

        _cardUIs = new();

        float offsetY = _offsetY;
        float offsetZ = _offsetZ;
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Card card = cards.ToList().ElementAt(i);
            StackCard(card, offsetY, offsetZ);
            offsetY += _offsetY;
            offsetZ += _offsetZ;
        }
        onDone?.Invoke();
    }

    private void StackCard(Card card, float offsetY, float offsetZ)
    {
        Utils.InstantiateAsync(Utils.CardPrefabAddress, transform, (go) =>
        {
            go.transform.localPosition += Vector3.up * offsetY;
            go.transform.localPosition += Vector3.back * offsetZ;
            CardUI cardUI = go.GetComponent<CardUI>();
            cardUI.Init(card, _controller);
            if (_cardUIs.Count > 0)
            {
                cardUI.transform.SetParent(_cardUIs.ElementAt(_cardUIs.Count - 1).transform);
            }
            _cardUIs.Add(cardUI);
        }, () => { });
    }

    public void RemoveTopmostCard(Action onDone)
    {
        CardUI lastCard = _cardUIs.Last();
        _cardUIs.Remove(lastCard);
        Destroy(lastCard.gameObject);

        if (_cardUIs.Count > 0)
        {
            CardUI next = _cardUIs.ElementAt(_cardUIs.Count - 1);
            next.Reveal(true);
        }
        onDone?.Invoke();
    }
}