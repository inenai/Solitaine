using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(Collider2D))]
    public class StockView : CardPileView, IClick
    {
        public override PileKind PileKind => PileKind.STOCK;

        [SerializeField] GameObject _restockLocked;
        [SerializeField] Transform _cardsRoot;

        protected override void OnInit() { }

        public override void Refresh(CardPile cards, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action onDone)
        {
            //Logs.Log("StockView refreshing...");
            if (_restockLocked != null)
                _restockLocked.SetActive(!_view.Controller.IsRestockAvailable());

            if (cards.Count == 0)
            {
                //Logs.Log("StockView refreshed.");
                onDone?.Invoke();
                return;
            }

            if (cardMoved == null)
            {
                StackCardsInPosition(cards, _cardsRoot.position, _cardsRoot.transform, "Card_S_");
                //Logs.Log("StockView refreshed.");
                onDone?.Invoke();
                return;
            }

            StackCardsInPositionWithAnimation(cards, _cardsRoot.position, _cardsRoot.transform, "Card_S_", cardMoved, originalCardPosition, onDone);
        }

        public void OnClick()
        {
            Logs.Log($"Stock pile clicked!");
            _view.Controller.InputAction_PileClicked(PileKind.STOCK, -1);
        }

        public bool CanClick()
        {
            return true;
        }

        public void OnClickAttemptFailed(){ }
    }
}