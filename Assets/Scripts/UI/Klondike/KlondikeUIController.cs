using System;
using UnityEngine;
using UI.Common;
using static Model.Common.Enums;
using Model.Common;
using System.Linq;
using System.Collections.Generic;
using Common.Utils;

namespace Klondike
{
    public class KlondikeUIController : MonoBehaviour, IGameUIController
    {
        [SerializeField] StockUI _stock;
        [SerializeField] WasteUI _waste;
        [SerializeField] FoundationUI[] _foundations;
        [SerializeField] TableauUI[] _tableaus;

        private KlondikeGame _game;
        public void StartGame(KlondikeGame game, Action onReady)
        {
            _game = game;
            _stock.Init(Utils.Clone(_game.State.StockPile),this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationUI foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i, Utils.Clone(_game.State.Tableau[i]));
            }
            RefreshStock();
            onReady?.Invoke();
        }

        private void RefreshStock(Action onDone = null)
        {
            _stock.Refresh(Utils.Clone(_game.State.StockPile), onDone);
        }

        private void RefreshWaste(Action onDone)
        {
            _waste.Refresh(Utils.Clone(_game.State.WastePile), onDone);
        }

        private void RefreshFoundations(Action onDone)
        {
            for (int i = 0; i < _foundations.Length; i++)
            {
                _foundations[i].Refresh(Utils.Clone(_game.State.Foundations[i].Stack), onDone);
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

            Func<bool> action = pileData.Kind switch
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
                _tableaus[index].RemoveTopmostCard(() =>
                {
                    RefreshFoundations(_game.UIDoneRefreshing);
                });
                return true;
            }
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
                return true;
            }
            return false;
        }
    }
}