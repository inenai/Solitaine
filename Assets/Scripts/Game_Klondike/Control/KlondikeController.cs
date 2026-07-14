using Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using static Utils.Utils;

namespace Klondike
{
    public class KlondikeController : MonoBehaviour, IGameController
    {
        [SerializeField] MyInputManager _input;
        [SerializeField] KlondikeConfig _defaultConfig;
        [SerializeField] KlondikeView _view;
        [SerializeField] KlondikeUI _ui;

        private List<Card> _deck;
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
            Initialize(() =>
            {
                StartNewGame(() =>
                    {
                        Status = GameStatus.LISTENING;
                    });
            });

        }

        void OnDestroy()
        {
            DeregisterFromEvents();
        }

        private void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }

        private void Initialize(Action onDone)
        {
            _deck = KlondikeGame.CreateGameDeck();
            RegisterToEvents();
            InitConfig();
            _ui.Init(this);
            _view.Init(this);
            LoadDeckView(onDone);
        }

        private void StartNewGame(Action onDone)
        {
            _ui.OnStartNewGame();
            ResetDeck();
            _game = new KlondikeGame(_deck);
            RefreshView(new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU, PileKind.WASTE, PileKind.FOUNDATION }, null, default, onDone);
        }

        private void ResetDeck()
        {
            foreach (Card card in _deck)
            {
                card.Show(false);
                card.FreeCard(false);
            }
        }

        private async void LoadDeckView(Action onDone)
        {
            Debug.Log("LoadDeckView.");
            await _view.Deck.Load(_deck);
            onDone?.Invoke();
        }
        #endregion

        #region GameController
        public void RestartGame()
        {
            if (Status != GameStatus.LISTENING) return;
            Status = GameStatus.PROCESSING;
            StartNewGame(() =>
            {
                Status = GameStatus.LISTENING;
            });
        }

        public void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        public bool PileClicked(PileKind pileKind, int pileIndex)
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

            return RefreshView(
                action.Invoke(), null, default, () =>
                {
                    Status = GameStatus.LISTENING;
                });
        }


        public bool CardDoubleClicked(Card card, Vector3 originalCardPosition)
        {
            Debug.Log("Processing double click.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return RefreshView(
                _game.Action_TryMoveCardAutomatic(card), card, originalCardPosition, () =>
                {
                    Status = GameStatus.LISTENING;
                });
        }

        public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex, Vector3 originalCardPosition)
        {
            Debug.Log("Processing card dragged to pile.");
            if (Status != GameStatus.LISTENING) return false;
            Status = GameStatus.PROCESSING;

            return RefreshView(
               _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex), card, originalCardPosition, () =>
                {
                    Status = GameStatus.LISTENING;
                });
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

        public bool IsCardInTargetPile(Card card, out TargetCardPileView result)
        {
            return _view.IsCardInTargetPile(_game.State.GetCardPileOwnerData(card), out result);
        }
        #endregion

        #region UI
        public void OpenSettings()
        {
            _input.Pause(true);
            _ui.OpenSettings();
        }

        public void CloseSettings()
        {
            _input.Pause(false);
            _ui.CloseSettings();
        }
        #endregion

        #region View
        private bool RefreshView(List<PileKind> pilesToRefresh, Card cardMoved, Vector3 originalCardPosition, Action onDone)
        {
            Debug.Log("RefreshView.");
            bool gameStateChanged = pilesToRefresh.Count > 0;
            if (gameStateChanged)
            {
                _ = RefreshViewTask(pilesToRefresh, cardMoved, originalCardPosition, onDone);
            }
            else
            {
                onDone?.Invoke();
            }
            return gameStateChanged;
        }

        private async Task RefreshViewTask(List<PileKind> pilesToRefresh, Card cardMoved, Vector3 originalCardPosition, Action onDone)
        {
            Debug.Log("RefreshViewTask.");
            List<Task> tasks = new();
            foreach (PileKind kind in pilesToRefresh)
            {
                switch (kind)
                {
                    case PileKind.WASTE:
                        tasks.Add(_view.RefreshWaste(CloneStack(_game.State.WastePile), cardMoved, originalCardPosition));
                        break;

                    case PileKind.STOCK:
                        tasks.Add(_view.RefreshStock(CloneStack(_game.State.StockPile), cardMoved, originalCardPosition));
                        break;

                    case PileKind.FOUNDATION:
                        tasks.Add(_view.RefreshFoundations(CardUtils.GetClonedStacks(_game.State.Foundations), cardMoved, originalCardPosition));
                        break;

                    case PileKind.TABLEAU:
                        tasks.Add(_view.RefreshTableaus(CardUtils.GetClonedStacks(_game.State.Tableaus), cardMoved, originalCardPosition));
                        break;
                }
            }
            Debug.Log("Await...");
            await Task.WhenAll(tasks);
            Debug.Log("Done.");
            onDone?.Invoke();
        }

        #endregion

        #region Events
        private void RegisterToEvents()
        {
            EventManager.OnResetGameEvent += OnResetGameEvent;
            EventManager.OnDrawFromStockEvent += OnDrawFromStockEvent;
            EventManager.OnGameWon += OnGameWon;
        }

        private void DeregisterFromEvents()
        {
            EventManager.OnResetGameEvent -= OnResetGameEvent;
            EventManager.OnDrawFromStockEvent -= OnDrawFromStockEvent;
            EventManager.OnGameWon -= OnGameWon;
        }

        private void OnResetGameEvent()
        {
            RestartGame();
        }

        private void OnDrawFromStockEvent()
        {
            PileClicked(PileKind.STOCK, -1);
        }

        private void OnGameWon()
        {
            KlondikeSettings.WinCount++;
            _ui.OnGameWon();
        }
        #endregion
    }
}