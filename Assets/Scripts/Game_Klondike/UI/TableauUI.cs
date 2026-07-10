using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using UnityEngine;
using UnityEngine.AI;
using Utils;

namespace Klondike
{
    public class TableauUI : TargetCardPileUI
    {
        private const float _offsetY = -0.3f;
        private const float _offsetZ = -0.1f;

        private List<CardUI> _cardUIs;

        protected override void OnInit()
        {
            _cardUIs = new();
        }

        private async Task<CardUI> CreateCardUI(Transform transform)
        {
            GameObject go = await AssetManager.InstantiateAsync(CardUtils.CardPrefabAddress, transform);
            CardUI cardUI = go.GetComponent<CardUI>();
            cardUI.Init(_controller);
            return cardUI;
        }

        public override async Task Refresh(Stack<Card> tableauCards)
        {
            Card[] cards = tableauCards.Reverse().ToArray();
            RemoveExtraCardUIs(cards.Length);

            List<Task<CardUI>> tasks = new();

            for (int i = _cardUIs.Count; i < cards.Length; i++)
            {
                tasks.Add(CreateCardUI(transform));
            }

            CardUI[] newCards = await Task.WhenAll(tasks);

            _cardUIs.AddRange(newCards);

            if (_cardUIs.Count != cards.Length)
                throw new Exception("Missmatch in tableau cards amount after initializing new cards");

            for (int i = 0; i < cards.Length; i++)
            {
                Transform desiredParent = i == 0 ? transform : _cardUIs[i - 1].transform;
                if (_cardUIs[i].transform.parent != desiredParent)
                {
                    _cardUIs[i].transform.SetParent(_cardUIs[i - 1].transform);
                }
                float yOffset = i == 0 ? 0f : _offsetY;
                _cardUIs[i].LoadCardData(cards[i], yOffset, _offsetZ);
            }

            triggerHighlight.transform.position = new Vector3(triggerHighlight.transform.position.x, GetHighlightYPos(), -0.1f * (_cardUIs.Count + 1));
            triggerHighlight.transform.localScale = new Vector3(1f, GetHighlightYScale(),1f);
        }

        private float GetHighlightYPos()
        {
            if (_cardUIs.Count <= 1)
            {
                return transform.position.y;
            }

            return transform.position.y + _offsetY * (_cardUIs.Count - 1) / 2f;
        }

        private float GetHighlightYScale()
        {
            if (_cardUIs.Count == 0) return 1.5f;
            return 1.5f + (-_offsetY) * (_cardUIs.Count - 1);
        }

        private void RemoveExtraCardUIs(int amountNeeded)
        {
            if (_cardUIs.Count <= amountNeeded) return;

            while (_cardUIs.Count > amountNeeded)
            {
                CardUI last = _cardUIs[^1];
                Destroy(last.gameObject);
                _cardUIs.RemoveAt(_cardUIs.Count - 1);
            }
        }
    }
}