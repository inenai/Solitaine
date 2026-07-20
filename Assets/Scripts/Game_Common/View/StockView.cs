using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(Collider2D))]
    public class StockView : TargetCardPileView, IClick
    {
        [SerializeField] GameObject _restockLocked;
        [SerializeField] Transform _cardsRoot;

        public Transform CardsRootTr => _cardsRoot;
        protected override void OnInit() { }

        public override void Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            Debug.Log("StockView refreshing...");
            if (_restockLocked != null)
                _restockLocked.SetActive(!_view.Controller.IsRestockAvailable());

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