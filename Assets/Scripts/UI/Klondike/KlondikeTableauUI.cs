using System.Collections.Generic;
using System.Linq;
using Common;
using Klondike;
using Model.Common;
using UnityEngine;

public class KlondikeTableauUI : MonoBehaviour
{
    private float _offsetY = -0.3f;
    private float _offsetZ = 0.1f;
    private KlondikeUI _master;
    private int _tableauIndex = 0;
    private List<CardUI> _cardUIs;

    public void Init(KlondikeUI master, int index, Stack<Card> cards)
    {
        Debug.Log($"Init tableau [{index}] with {cards.Count} cards");
        _cardUIs = new();
        _tableauIndex = index;
        _master = master;
        float offsetY = _offsetY;
        float offsetZ = _offsetZ;
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            Card card = cards.ToList().ElementAt(i);
            StackCard(card, offsetY, offsetZ);
            offsetY += _offsetY;
            offsetZ += _offsetZ;
        }
    }

    private void StackCard(Card card, float offsetY, float offsetZ)
    {
        Utils.InstantiateAsync(Utils.CardPrefabAddress, transform, (go) =>
        {
            go.transform.localPosition += Vector3.up * offsetY;
            go.transform.localPosition += Vector3.back * offsetZ;
            CardUI cardUI = go.GetComponent<CardUI>();
            cardUI.Init(card, this);
            cardUI.GetComponent<Collider2D>().enabled = card.Revealed;
            _cardUIs.Add(cardUI);
        }, () => { });
    }

    public void CardDoublePressed()
    {
        bool success = _master.PilePressed(Enums.PileKind.TABLEAU, _tableauIndex);
        if (success)
        {
            GameObject go = _cardUIs.ElementAt(_cardUIs.Count - 1).gameObject;
            _cardUIs.RemoveAt(_cardUIs.Count - 1);
            Destroy(go);
            if (_cardUIs.Count > 0)
            {
                _cardUIs.ElementAt(_cardUIs.Count - 1).Reveal(true);
            }
        }
    }
}