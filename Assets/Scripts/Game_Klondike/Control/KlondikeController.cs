using Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        #region Initialization
        void Start()
        {
            InitConfig();
            InitGame();
            InitUI();
            FirstLoadUI(_game.OnUISetup);
        }

        private void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                KlondikeSettings.Reset(_defaultConfig);
            }
        }

        private void InitGame()
        {
            _game = new KlondikeGame();
            _game.SetupGame();
        }

        public void InitUI()
        {
            _stock.Init(this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationUI foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i);
            }
        }

        private void FirstLoadUI(Action onDone)
        {
            RefreshTableaus();
            RefreshStock();
            onDone?.Invoke();
        }
        #endregion

        #region GameController
        public void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        public bool PileClicked(PileKind pileKind, int index)
        {
            Func<List<PileKind>> action = pileKind switch
            {
                PileKind.STOCK => _game.Action_TryDrawCardsFromStock,
                _ => null
            };

            if (action == null)
            {
                _game.UIDoneRefreshing();
                return false;
            }

            List<PileKind> result = action.Invoke();
            RefreshPileUI(result, () =>
            {
                _game.UIDoneRefreshing();
            });
            return result.Count > 0;
        }

        public bool CardDoubleClicked(Card card)
        {
            Debug.Log("Processing double click.");

            List<PileKind> result = _game.Action_TryMoveCardAutomatic(card);
            bool cardsMoved = result.Count > 0;
            if (cardsMoved)
            {
                RefreshPileUI(result, () =>
                {
                    _game.UIDoneRefreshing();
                });
            }
            return cardsMoved;
        }

        public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            List<PileKind> result = _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex);
            bool cardsMoved = result.Count > 0;
            if (cardsMoved)
            {
                RefreshPileUI(result, () =>
                {
                    _game.UIDoneRefreshing();
                });
            }
            return cardsMoved;
        }

        public bool IsRestockAvailable(PileKind pileKind)
        {
            if (pileKind != PileKind.STOCK) return false;

            return _game.State.AvailableRestocks != 0;
        }

        public bool IsCardAllowedHere(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
        }
        #endregion

        #region UI
        private void RefreshPileUI(List<PileKind> toRefresh, Action onDone = null)
        {
            foreach (PileKind kind in toRefresh)
            {
                //TODO when this waiting for animations, wait for all to be done before continuing (async Tasks?)
                switch (kind)
                {
                    case PileKind.WASTE:
                        RefreshWaste();
                        break;
                    case PileKind.STOCK:
                        RefreshStock();
                        break;
                    case PileKind.FOUNDATION:
                        RefreshFoundations();
                        break;
                    case PileKind.TABLEAU:
                        RefreshTableaus();
                        break;
                }
            }
            onDone?.Invoke();
        }

        private void RefreshStock(Action onDone = null)
        {
            _stock.Refresh(CloneStack(_game.State.StockPile),onDone);
        }

        private void RefreshWaste(Action onDone = null)
        {
            _waste.Refresh(CloneStack(_game.State.WastePile), onDone);
        }

        private void RefreshFoundations(Action onDone = null)
        {
            for (int i = 0; i < _foundations.Length; i++)
            {
                _foundations[i].Refresh(CloneStack(_game.State.Foundations[i].Stack), onDone);
            }
        }

        private void RefreshTableaus(Action onDone = null)
        {
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Refresh(CloneStack(_game.State.Tableaus[i]), onDone);
            }
        }
        #endregion
    }
}