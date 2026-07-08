using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

namespace Klondike
{
    public class TableauUI : TargetCardPileUI
    {
        private const float _offsetY = -0.3f;
        private const float _offsetZ = 0.1f;
        private List<CardUI> _cardUIs;

        protected override void OnInit()
        {
            _cardUIs = new();
        }

        public void Refresh(Stack<Card> cards, Action onDone)
        {
            cards = new Stack<Card>(cards); //InvertOrder
            RemoveExtraCardUIs(cards.Count);

            int index = 0;
            while (cards.Count > 0)
            {
                Card cardToLoad = cards.Pop();
                if (_cardUIs.Count >= index + 1)
                {
                    _cardUIs[index].LoadCardData(cardToLoad, index);
                }
                else
                {
                    StackCard(index, cardToLoad, null);
                }
                index++;
            }
            onDone?.Invoke();
        }

        private void StackCard(int index, Card card, Action<CardUI> callback) //Tercera carta se puso tan abajo como si fuera la 4ta y tan adelante como si fuera la 5ta
        {
            float offsetY = 0f + (_offsetY * index);
            float offsetZ = _offsetZ + (_offsetZ * index);

            Transform parentTr = _cardUIs.Count > 0 ?
                _cardUIs.ElementAt(_cardUIs.Count - 1).transform :
                transform;

            CreateCardUI(parentTr,(gameObject) =>
            {
                gameObject.transform.localPosition += Vector3.up * offsetY;
                gameObject.transform.localPosition += Vector3.back * offsetZ;
                CardUI cardUI = gameObject.GetComponent<CardUI>();
                if (cardUI == null) throw new Exception("Fatal error: CardUI prefab does not contain CardUI component!");
                _cardUIs.Add(cardUI);
                cardUI.Init(_controller);
                callback?.Invoke(cardUI);
                cardUI.LoadCardData(card);
            });
        }

        private void CreateCardUI(Transform parentTr, Action<GameObject> onDone)
        {
            AssetManager.InstantiateAsync(CardUtils.CardPrefabAddress, parentTr, (go) =>
            {
                onDone?.Invoke(go);
            }, () => { });
        }

        private void RemoveExtraCardUIs(int amountNeeded)
        {
            if (_cardUIs.Count <= amountNeeded) return;

            while (_cardUIs.Count > amountNeeded)
            {
                Destroy(_cardUIs.ElementAt(_cardUIs.Count - 1).gameObject);
                _cardUIs.Remove(_cardUIs.ElementAt(_cardUIs.Count - 1));
            }
        }
    }
}