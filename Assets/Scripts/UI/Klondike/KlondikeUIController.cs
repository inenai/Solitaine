using System;
using UnityEngine;
using UI.Common;
using static Model.Common.Enums;
using Model.Common;

namespace Klondike
{
    public class KlondikeUIController : MonoBehaviour, IGameUIController
    {
        [SerializeField] StockUI _stock;
        [SerializeField] WasteUI _waste;
        [SerializeField] FoundationUI[] _foundations;
        [SerializeField] TableauUI[] _tableaus;

        private KlondikeGame _game;

        public void Setup(KlondikeGame game)
        {
            _game = game;
            _stock.Init(this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationUI foundation = _foundations[i];
                foundation.Init(this, i);
            }
        }

        public void StartGame()
        {
            RefreshStock(null);
            RefreshWaste(null);
            LoadTableaus();
        }

        private void LoadTableaus()
        {
            for (int i = 0; i < _tableaus.Length; i++)
            {
                TableauUI tableau = _tableaus[i];
                tableau.Init(this, i, _game.State.Tableau[i]);
            }
        }

        private void RefreshStock(Action onDone)
        {
            _stock.Refresh(_game.State.StockPile, onDone);
        }

        private void RefreshWaste(Action onDone)
        {
            _waste.Refresh(_game.State.WastePile, onDone);
        }

        private void RefreshFoundations(Action onDone)
        {
            for (int i = 0; i < _foundations.Length; i++)
            {
                _foundations[i].Refresh(_game.State.Foundations[i].Stack, onDone);
            }
        }

        // public bool CardDraggedTo(Card card, PileKind targetPile, int targetPileIndex)
        // {

        // }

        public bool PileClicked(PileKind pileKind, int index)
        {
            Func<bool> action = pileKind switch
            {
                PileKind.STOCK => _game.Action_DrawCardsFromStock,
                _ => null
            };

            if (action == null)
            {
                _game.UIDoneRefreshing();
                return false;
            }

            if (action.Invoke())
            {
                RefreshStock(() =>
                {
                    RefreshWaste(() =>
                    {
                        _game.UIDoneRefreshing();
                    });
                });
                return true;
            }
            return false;
        }

        public bool CardDoubleClicked(Card card)
        {
            Debug.Log("Processing double click.");
            CardPileData pileData = _game.State.GetCardPileOwnerData(card);

            Func<bool> action = pileData.PileKind switch
            {
                PileKind.WASTE => WasteDoubleClicked,
                PileKind.TABLEAU => () => TableauDoubleClicked(pileData.Index),
                _ => null
            };

            if (action == null)
            {
                _game.UIDoneRefreshing();
                return false;
            }

            return action.Invoke();
        }

        private bool TableauDoubleClicked(int index)
        {
            Debug.Log("Tableau card was double clicked.");
            if (_game.Action_MoveFromTableauToFoundation(index))
            {
                RefreshFoundations(() =>
                {
                    _game.UIDoneRefreshing();
                });
                Debug.Log("Yup.");
                return true;
            }
            Debug.Log("Nope.");
            return false;
        }

        private bool WasteDoubleClicked()
        {
            Debug.Log("Waste card was double clicked.");
            if (_game.Action_MoveFromWasteToAutoFoundation())
            {
                RefreshWaste(() =>
                {
                    RefreshFoundations(() =>
                    {
                        _game.UIDoneRefreshing();
                    });
                });
                Debug.Log("Yup.");
                return true;
            }
            Debug.Log("Nope.");
            return false;
        }
    }
}