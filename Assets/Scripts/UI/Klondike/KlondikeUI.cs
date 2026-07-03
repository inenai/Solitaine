using System;
using UnityEngine;
using UI.Common;
using static Model.Common.Enums;

namespace Klondike
{
    public class KlondikeUI : IGameUIController
    {
        KlondikeGame _game;
        [SerializeField] StockUI _stock;
        [SerializeField] StockUI _waste;
        [SerializeField] FoundationUI[] _foundations;
        [SerializeField] KlondikeTableauUI[] _tableaus;

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
                KlondikeTableauUI tableau = _tableaus[i];
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

        public bool PilePressed(PileKind pileKind, int index = 0)
        {
            Func<bool> action = pileKind switch
            {
                PileKind.STOCK => _game.Action_DrawCardsFromStock,
                PileKind.WASTE => _game.Action_MoveFromWasteToAutoFoundation,
                PileKind.TABLEAU => () => _game.Action_MoveFromTableauToFoundation(index),
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
                        RefreshFoundations(() =>
                        {
                            _game.UIDoneRefreshing();
                        });
                    });
                });
                return true;
            }
            return false;
        }
    }
}