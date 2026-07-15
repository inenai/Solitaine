using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    [RequireComponent(typeof(Collider2D))]
    public class StockView : CardPileView, IClick
    {
        [SerializeField] GameObject _restockLocked;
        [SerializeField] Transform _cardsRoot;

        protected override void OnInit() {}

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, Action onDone)
        {
            Debug.Log("StockView refreshing...");
            _restockLocked.SetActive(!_view.Controller.IsRestockAvailable(PileKind.STOCK));
            if (cards.Count == 0)
            {
                Debug.Log("StockView refreshed.");
                onDone?.Invoke();
                return;
            }

            StackCardsInPosition(cards, _cardsRoot.position, transform, "Card_S_");

            Debug.Log("StockView refreshed.");
            onDone?.Invoke();
        }
        public void OnClick()
        {
            Debug.Log($"Stock pile clicked!");
            _view.Controller.PileClicked(PileKind.STOCK, -1);
        }
        public bool CanClick()
        {
            return true;
        }

        public void OnClickAttemptFailed(){ }
    }
}