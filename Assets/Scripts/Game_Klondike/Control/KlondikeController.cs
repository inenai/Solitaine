using Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Utils.Utils;

namespace Klondike
{
    public class KlondikeController : MonoBehaviour, IGameController
    {
        [SerializeField] MyInputManager _input;
        [SerializeField] KlondikeConfig _defaultConfig;
        [SerializeField] StockUI _stock;
        [SerializeField] WasteUI _waste;
        [SerializeField] FoundationUI[] _foundations;
        [SerializeField] TableauUI[] _tableaus;
        [SerializeField] ParticleSystem _victoryParticles;
        [SerializeField] GameObject _settingsScreen;
        [SerializeField] TextMeshProUGUI _winsTxt;

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
            InitUI();
            StartNewGame();
        }

        private void StartNewGame()
        {
            _victoryParticles.Stop();
            ClearGame();
            _game = new KlondikeGame();
            _game.OnWin += OnGameWon;
            RefreshAllUI();
        }

        private void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }

        private void ClearGame()
        {
            if (_game != null)
                _game.OnWin -= OnGameWon;
        }

        public void InitUI()
        {
            _winsTxt.text = $"Wins: {KlondikeSettings.WinCount}";
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

        private void RefreshAllUI()
        {
            _ = FinishRefresh(new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU, PileKind.WASTE, PileKind.FOUNDATION }, null, default);
        }

        private void OnGameWon()
        {
            KlondikeSettings.WinCount++;
            _winsTxt.text = $"Wins: {KlondikeSettings.WinCount}";
            _victoryParticles.Play();
        }

        #endregion

        #region GameController
        public void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
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
                action.Invoke(),null,default);
        }

        public bool CardDoubleClicked(Card card, Vector3 originalCardPosition)
        {
            Debug.Log("Processing double click.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return UpdateUIWithChanges(
                _game.Action_TryMoveCardAutomatic(card), card, originalCardPosition);
        }

        public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex, Vector3 originalCardPosition)
        {
            Debug.Log("Processing card dragged to pile.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return UpdateUIWithChanges(
               _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex), card, originalCardPosition);
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
        public void RestartGame()
        {
            StartNewGame();
        }

        public void OpenSettings()
        {
            _input.Pause(true);
            _settingsScreen.SetActive(true);
        }

        public void CloseSettings()
        {
            _input.Pause(false);
            _settingsScreen.SetActive(false);
        }
        #endregion

        #region UIInternal
        private bool UpdateUIWithChanges(List<PileKind> pilesToRefresh, Card cardMoved, Vector3 originalCardPosition)
        {
            bool moved = pilesToRefresh.Count > 0;
            if (moved)
            {
                _ = FinishRefresh(pilesToRefresh, cardMoved, originalCardPosition);
            }
            else
            {
                Status = GameStatus.LISTENING;
            }
            return moved;
        }

        private async Task FinishRefresh(List<PileKind> pilesToRefresh, Card cardMoved, Vector3 originalCardPosition)
        {
            try
            {
                await RefreshPileUI(pilesToRefresh, cardMoved, originalCardPosition);
            }
            finally
            {
                Status = GameStatus.LISTENING;
            }
        }

        private async Task RefreshPileUI(List<PileKind> toRefresh, Card cardMoved, Vector3 originalCardPosition)
        {
            List<Task> tasks = new();

            foreach (PileKind kind in toRefresh)
            {
                switch (kind)
                {
                    case PileKind.WASTE:
                        tasks.Add(RefreshWaste(cardMoved, originalCardPosition));
                        break;

                    case PileKind.STOCK:
                        tasks.Add(RefreshStock(cardMoved, originalCardPosition));
                        break;

                    case PileKind.FOUNDATION:
                        tasks.Add(RefreshFoundations(cardMoved, originalCardPosition));
                        break;

                    case PileKind.TABLEAU:
                        tasks.Add(RefreshTableaus(cardMoved, originalCardPosition));
                        break;
                }
            }

            await Task.WhenAll(tasks);
        }

        private Task RefreshStock(Card cardMoved, Vector3 originalCardPosition)
        {
            return _stock.Refresh(CloneStack(_game.State.StockPile), cardMoved, originalCardPosition);
        }

        private Task RefreshWaste(Card cardMoved, Vector3 originalCardPosition)
        {
            return _waste.Refresh(CloneStack(_game.State.WastePile), cardMoved, originalCardPosition);
        }

        private Task RefreshFoundations(Card cardMoved, Vector3 originalCardPosition)
        {
            List<Task> tasks = new();
            for (int i = 0; i < _foundations.Length; i++)
            {
                tasks.Add(_foundations[i].Refresh(CloneStack(_game.State.Foundations[i].Stack), cardMoved, originalCardPosition));
            }
            return Task.WhenAll(tasks);
        }

        private Task RefreshTableaus(Card cardMoved, Vector3 originalCardPosition)
        {
            List<Task> tasks = new();
            for (int i = 0; i < _tableaus.Length; i++)
            {
                tasks.Add(_tableaus[i].Refresh(CloneStack(_game.State.Tableaus[i]), cardMoved, originalCardPosition));
            }
            return Task.WhenAll(tasks);
        }

        public bool CardDraggedToPile(Card card, PileKind pileKind, int pileIndex, System.Numerics.Vector3 originalCardPosition)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}