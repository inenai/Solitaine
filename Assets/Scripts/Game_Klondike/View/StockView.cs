using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;
using Utils;

namespace Klondike
{
    [RequireComponent(typeof(Collider2D))]
    public class StockView : CardPileView, IClick
    {
        [SerializeField] GameObject _restockLocked;
        [SerializeField] Transform _cardsRoot;

        protected override void OnInit() {}

        public override Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            Debug.Log("StockView refreshing...");
            _restockLocked.SetActive(!_view.Controller.IsRestockAvailable(PileKind.STOCK));
            if (cards.Count == 0)
            {
                Debug.Log("StockView refreshed.");
                return Task.CompletedTask;
            }

            StackCardsInPosition(cards, _cardsRoot.position, transform, "Card_S_");

            Debug.Log("StockView refreshed.");
            return Task.CompletedTask;
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