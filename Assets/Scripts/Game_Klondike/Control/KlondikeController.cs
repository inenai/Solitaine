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
                _defaultConfig.ApplyDefaultSettings();
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

        private bool UpdateUIWithChanges(List<PileKind> pilesToRefresh)
        {
            bool moved = pilesToRefresh.Count > 0;
            if (moved)
            {
                _ = FinishRefresh(pilesToRefresh);
            }
            return moved;
        }

        private async Task FinishRefresh(List<PileKind> pilesToRefresh)
        {
            try
            {
                await RefreshPileUI(pilesToRefresh);
            }
            finally
            {
                _game.UIDoneRefreshing();
            }
        }

        public bool PileClicked(PileKind pileKind, int index)
        {
            Debug.Log("Processing pile clicked.");
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

            return UpdateUIWithChanges(
                action.Invoke());
        }

        public bool CardDoubleClicked(Card card)
        {
            Debug.Log("Processing double click.");
            return UpdateUIWithChanges(
                _game.Action_TryMoveCardAutomatic(card));
        }

        public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Debug.Log("Processing card dragged to pile.");
            return UpdateUIWithChanges(
               _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex));
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
        private async Task RefreshPileUI(List<PileKind> toRefresh)
        {
            List<Task> tasks = new();

            foreach (PileKind kind in toRefresh)
            {
                switch (kind)
                {
                    case PileKind.WASTE:
                        tasks.Add(RefreshWaste());
                        break;

                    case PileKind.STOCK:
                        tasks.Add(RefreshStock());
                        break;

                    case PileKind.FOUNDATION:
                        tasks.Add(RefreshFoundations());
                        break;

                    case PileKind.TABLEAU:
                        tasks.Add(RefreshTableaus());
                        break;
                }
            }

            await Task.WhenAll(tasks);
        }

        private Task RefreshStock()
        {
            return _stock.Refresh(CloneStack(_game.State.StockPile));
        }

        private Task RefreshWaste()
        {
            return _waste.Refresh(CloneStack(_game.State.WastePile));
        }

        private Task RefreshFoundations()
        {
            List<Task> tasks = new();
            for (int i = 0; i < _foundations.Length; i++)
            {
                tasks.Add(_foundations[i].Refresh(CloneStack(_game.State.Foundations[i].Stack)));
            }
            return Task.WhenAll(tasks);
        }

        private Task RefreshTableaus()
        {
            List<Task> tasks = new();
            for (int i = 0; i < _tableaus.Length; i++)
            {
                tasks.Add(_tableaus[i].Refresh(CloneStack(_game.State.Tableaus[i])));
            }
            return Task.WhenAll(tasks);
        }
        #endregion
    }
}