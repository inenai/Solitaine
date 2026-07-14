using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;

namespace Klondike
{
    public class WasteView : CardPileView
    {
        /// <summary>
        /// <para> index => gameobject</para>
        /// <para>0 => 1st card, leftmost</para>
        /// <para>1 => 2nd card, middle</para>
        /// <para>2 => 3rd card, rightmost</para>
        /// </summary>
        [SerializeField] CardView[] _cardUIs;

        private Vector3[] _cardPositions;

        void Awake()
        {
            _cardPositions = new Vector3[_cardUIs.Length];
            for (int i = 0; i < _cardUIs.Length; i++)
            {
                _cardPositions[i] = _cardUIs[i].transform.position;
            }
        }

        protected override void OnInit()
        {
            for (int i = 0; i < _cardUIs.Length; i++)
            {
                _cardUIs[i].Init(_controller);
            }
        }

        private void ResetPositions()
        {
            for (int i = 0; i < _cardUIs.Length; i++)
            {
                _cardUIs[i].transform.position = _cardPositions[i];
            }
        }

        public override Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            if (_cardUIs.Length < 1)
            {
                throw new Exception("Waste has no card UI available in scene!");
            }
            ResetPositions();
            TurnOffUnusedCardUIs(cards.Count);
            RefreshWasteCardUIs(cards);

            return Task.CompletedTask;
        }

        private void RefreshWasteCardUIs(Stack<Card> waste)
        {
            int j = waste.Count > 2 ? 2 : waste.Count - 1;
            while (waste.Count > 0 && j >= 0)
            {
                CardView cardUI = _cardUIs[j];
                Debug.Log($"Activating waste card index {j}.");
                cardUI.gameObject.SetActive(true);
                cardUI.LoadCardData(waste.Pop());
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
}