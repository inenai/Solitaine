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
        [SerializeField] ParticleSystem _victoryParticles;

        private KlondikeGame _game;
        private GameStatus _status = GameStatus.INITIALIZING;

        GameStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                Debug.Log($"STATUS {value}");
            }
        }

        #region Initialization
        void Start()
        {
            Status = GameStatus.INITIALIZING;
            InitConfig();
            InitGame();
            InitUI();
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
            _game.OnWin += OnGameWon;
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
            _ = FinishRefresh(new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU });
        }
        #endregion

        #region GameController
        public void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        private void OnGameWon()
        {
            _victoryParticles.Play();
        }

        public bool PileClicked(PileKind pileKind, int index)
        {
            Debug.Log("Processing pile clicked.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;
            Func<List<PileKind>> action = pileKind switch
            {
                PileKind.STOCK => _game.Action_TryDrawCardsFromStock,
                _ => null
            };

            if (action == null)
            {
                Status = GameStatus.LISTENING;
                return false;
            }

            return UpdateUIWithChanges(
                action.Invoke());
        }

        public bool CardDoubleClicked(Card card)
        {
            Debug.Log("Processing double click.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return UpdateUIWithChanges(
                _game.Action_TryMoveCardAutomatic(card));
        }

        public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Debug.Log("Processing card dragged to pile.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return UpdateUIWithChanges(
               _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex));
        }

        public bool IsRestockAvailable(PileKind pileKind)
        {
            if (pileKind != PileKind.STOCK) return false;

            return _game.State.AvailableRestocks != 0;
        }

        public bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
        }

        public bool IsCardInTargetPile(Card card, out TargetCardPileUI result)
        {
            result = null;
            PileData cardPileData = _game.State.GetCardPileOwnerData(card);
            switch (cardPileData.Kind)
            {
                case PileKind.WASTE:
                    return false;
                case PileKind.STOCK:
                    return false;
                case PileKind.FOUNDATION:
                    result = _foundations[cardPileData.Index];
                    return true;
                case PileKind.TABLEAU:
                    result = _tableaus[cardPileData.Index];
                    return true;
            }
            return false;
        }
        #endregion

        #region UI
        private bool UpdateUIWithChanges(List<PileKind> pilesToRefresh)
        {
            bool moved = pilesToRefresh.Count > 0;
            if (moved)
            {
                _ = FinishRefresh(pilesToRefresh);
            } else
            {
                Status = GameStatus.LISTENING;
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
                Status = GameStatus.LISTENING;
            }
        }

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