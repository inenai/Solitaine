using Common;
using System;
using UnityEngine;
using static Utils.Utils;

namespace Klondike
{
    public class KlondikeController : MonoBehaviour, IGameController
    {
        [SerializeField] KlondikeConfig _defaultConfig;
        [SerializeField] StockUI _stock;
        [SerializeField] WasteUI _waste;
        [SerializeField] FoundationUI[] _foundations;
        [SerializeField] TableauUI[] _tableaus;

        private KlondikeGame _game;

        void Start()
        {
            InitConfig();
            _game = new KlondikeGame();
            _game.SetupGame();
            InitUI(_game.OnUISetup);
        }

        private void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                KlondikeSettings.Reset(_defaultConfig);
            }
        }

        public void ResetKlondikeSettings()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        public void InitUI(Action onDone)
        {
            _stock.Init(CloneStack(_game.State.StockPile),this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationUI foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i, CloneStack(_game.State.Tableau[i]));
            }
            RefreshStock();
            onDone?.Invoke();
        }

        private void RefreshStock(Action onDone = null)
        {
            _stock.Refresh(CloneStack(_game.State.StockPile), onDone);
        }

        private void RefreshWaste(Action onDone)
        {
            _waste.Refresh(CloneStack(_game.State.WastePile), onDone);
        }

        private void RefreshFoundations(Action onDone)
        {
            for (int i = 0; i < _foundations.Length; i++)
            {
                _foundations[i].Refresh(CloneStack(_game.State.Foundations[i].Stack), onDone);
            }
        }

        #region InterfaceImplementation
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

        public bool IsRestockAvailable(PileKind pileKind)
        {
            if (pileKind != PileKind.STOCK) return false;

            return _game.State.AvailableRestocks != 0;
        }

        public bool IsCardAllowedHere(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanMoveCardToPile(card, targetPile, targetPileIndex);
        }
        #endregion

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