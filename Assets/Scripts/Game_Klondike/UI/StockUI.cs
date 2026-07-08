using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    [RequireComponent(typeof(Collider2D))]
    public class StockUI : CardPileUI, IClick
    {
        [SerializeField] GameObject _cardUI;
        [SerializeField] GameObject _restockLocked;

        protected override void OnInit() {}

        public void Refresh(Stack<Card> stock, Action onDone)
        {
            _cardUI.gameObject.SetActive(stock.Count > 0);
            _restockLocked.SetActive(!_controller.IsRestockAvailable(PileKind.STOCK));
            onDone?.Invoke();
        }

        public void OnClick()
        {
            Debug.Log($"Stock pile clicked!");
            _controller.PileClicked(PileKind.STOCK, -1);
        }
    }
}