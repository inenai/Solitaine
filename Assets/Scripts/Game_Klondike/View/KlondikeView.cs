using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;

namespace Klondike
{
    public class KlondikeView : GameView
    {
        //View
        [SerializeField] StockView _stock;
        [SerializeField] WasteView _waste;
        [SerializeField] FoundationView[] _foundations;
        [SerializeField] TableauView[] _tableaus;

        protected override void OnInit()
        {
            _stock.Init(this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationView foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i);
            }
        }

        public bool IsCardInTargetPile(PileData cardOwnerData, out TargetCardPileView result)
        {
            result = null;
            switch (cardOwnerData.Kind)
            {
                case PileKind.WASTE:
                    return false;
                case PileKind.STOCK:
                    return false;
                case PileKind.FOUNDATION:
                    result = _foundations[cardOwnerData.Index];
                    return true;
                case PileKind.TABLEAU:
                    result = _tableaus[cardOwnerData.Index];
                    return true;
            }
            return false;
        }

        public Task RefreshStock(Stack<Card> stock, Card cardMoved, Vector3 originalCardPosition)
        {
            return _stock.Refresh(stock, cardMoved, originalCardPosition);
        }

        public Task RefreshWaste(Stack<Card> waste, Card cardMoved, Vector3 originalCardPosition)
        {
            return _waste.Refresh(waste, cardMoved, originalCardPosition);
        }

        public Task RefreshFoundations(List<Stack<Card>> foundations, Card cardMoved, Vector3 originalCardPosition)
        {
            List<Task> tasks = new();
            for (int i = 0; i < _foundations.Length; i++)
            {
                tasks.Add(_foundations[i].Refresh(foundations[i], cardMoved, originalCardPosition));
            }
            return Task.WhenAll(tasks);
        }

        public Task RefreshTableaus(List<Stack<Card>> tableaus, Card cardMoved, Vector3 originalCardPosition)
        {
            List<Task> tasks = new();
            for (int i = 0; i < _tableaus.Length; i++)
            {
                tasks.Add(_tableaus[i].Refresh(tableaus[i], cardMoved, originalCardPosition));
            }
            return Task.WhenAll(tasks);
        }
    }
}