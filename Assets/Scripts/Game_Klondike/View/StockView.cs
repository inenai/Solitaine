using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;

namespace Klondike
{
    [RequireComponent(typeof(Collider2D))]
    public class StockView : CardPileView, IClick
    {
        [SerializeField] GameObject _cardUI;
        [SerializeField] GameObject _restockLocked;

        protected override void OnInit() {}

        public override Task Refresh(Stack<Card> cards, Card cardMoved, Vector3 originalCardPosition)
        {
            _cardUI.gameObject.SetActive(cards.Count > 0);
            _restockLocked.SetActive(!_controller.IsRestockAvailable(PileKind.STOCK));
            return Task.CompletedTask;
        }

        public void OnClick()
        {
            Debug.Log($"Stock pile clicked!");
            _controller.PileClicked(PileKind.STOCK, -1);
        }
    }
}