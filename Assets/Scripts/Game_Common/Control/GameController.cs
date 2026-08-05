using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public abstract class GameController : MonoBehaviour, IGameController
{
    [SerializeField] protected MyInputManager _input;
    [SerializeField] protected GameUI _ui;

    protected int _viewsRefreshing = 0;
    protected List<Card> _deck;
    protected Game _game;
    protected GameView _gameView;
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

    public abstract void ResetSettingsToDefault();
    public abstract bool IsRestockAvailable();
    public abstract bool IsAutoMovesEnabled();
    protected abstract void InitGameView();
    protected abstract void CreateDeck();
    protected abstract void CreateGame();
    protected abstract void ResetView();
    protected abstract void InitConfig();
    protected abstract void UpdateWinsCount();
    protected abstract void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false);

    public bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
    {
        return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
    }

    public bool IsCardInTargetPile(Card card, out TargetCardPileView result)
    {
        return _gameView.IsCardInTargetPile(_game.State.GetCardPileOwnerData(card), out result);
    }

    protected Card GetSolvableCard()
    {
        return _game.GetSolvableCard();
    }

    protected Vector3 GetCardViewPosition(Card card)
    {
        return _gameView.GetCardViewPosition(card);
    }

    protected void LoadDeckView(Action onDone)
    {
        Debug.Log("LoadDeckView.");
        _gameView.Deck.Load(_deck, onDone);
    }

    protected List<PileKind> AutoAction_MoveCardAutomatically(Card card)
    {
        return _game.AutoAction_TryMoveCardToFoundationAutomatically(card);
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

    private void Initialize(Action onDone)
    {
        CreateDeck();
        RegisterToEvents();
        InitConfig();
        _ui.Init(this);
        InitGameView();
        LoadDeckView(onDone);
    }

    private void StartNewGame(Action onDone)
    {
        _ui.OnStartNewGame();
        ResetDeck();
        ResetView();
        CreateGame();
        _game.Init();
        CheckRefreshView(new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU, PileKind.WASTE, PileKind.FOUNDATION }, null, default, onDone, true);
    }

    private void ResetDeck()
    {
        foreach (Card card in _deck)
        {
            card.Show(false);
            card.FreeCard(false);
        }
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

    public bool InputAction_PileClicked(PileKind pileKind, int pileIndex)
    {
        Debug.Log("Processing pile clicked.");
        if (Status != GameStatus.LISTENING) return false;
        Status = GameStatus.PROCESSING;

        return CheckRefreshView(
            _game.GameAction_ClickedPile(pileKind, pileIndex), null, default, () =>
            {
                Status = GameStatus.LISTENING;
            });
    }

    public bool InputAction_CardDoubleClicked(Card card, Vector3 originalCardPosition)
    {
        Debug.Log("Processing double click.");
        if (Status != GameStatus.LISTENING) return false;
        Status = GameStatus.PROCESSING;

        return CheckRefreshView(
            _game.GameAction_TrySmartMoveCard(card), card, originalCardPosition, () =>
            {
                Status = GameStatus.LISTENING;
            });
    }

    public bool InputAction_CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex, Vector3 originalCardPosition)
    {
        Debug.Log("Processing card dragged to pile.");
        if (Status != GameStatus.LISTENING) return false;
        Status = GameStatus.PROCESSING;

        return CheckRefreshView(
            _game.GameAction_TryMoveCardToPile(card, targetPileKind, targetPileIndex), card, originalCardPosition, () =>
            {
                Status = GameStatus.LISTENING;
            }, true);
    }
    #endregion

    #region View
    private bool CheckRefreshView(List<PileKind> pilesToRefresh, Card cardMoved, Vector3 originalCardPosition, Action onDone, bool immediate = false)
    {
        Debug.Log("RefreshView.");
        bool refreshNeeded = pilesToRefresh.Count > 0;
        if (refreshNeeded)
        {
            DoRefreshView(pilesToRefresh, () =>
            {
                if (IsAutoMovesEnabled())
                {
                    Card solvableCard = GetSolvableCard();
                    if (solvableCard == null)
                    {
                        Debug.Log("No auto moves availables.");
                        onDone?.Invoke();
                        return;
                    }

                    Vector3 originalSolvableCardPosition = GetCardViewPosition(solvableCard);
                    Debug.Log("Auto moves enabled. Solving automatic move.");
                    CheckRefreshView(
                        AutoAction_MoveCardAutomatically(solvableCard), solvableCard, originalSolvableCardPosition, onDone, false);
                }
                else
                {
                    Debug.Log("Auto moves not enabled.");
                    onDone?.Invoke();
                }
            }, cardMoved, originalCardPosition, immediate);
        }
        else
        {
            onDone?.Invoke();
        }
        return refreshNeeded;
    }

    #endregion

    #region Events
    private void RegisterToEvents()
    {
        EventManager.OnResetGameEvent += OnResetGameEvent;
        EventManager.OnDrawFromStockEvent += OnDrawFromStockEvent;
        EventManager.OnGameWon += OnGameWon;
        EventManager.OnMenuClosed += OnMenuClosed;
        EventManager.OnMenuOpened += OnMenuOpened;
    }

    private void DeregisterFromEvents()
    {
        EventManager.OnResetGameEvent -= OnResetGameEvent;
        EventManager.OnDrawFromStockEvent -= OnDrawFromStockEvent;
        EventManager.OnGameWon -= OnGameWon;
        EventManager.OnMenuClosed -= OnMenuClosed;
        EventManager.OnMenuOpened -= OnMenuOpened;
    }

    private void OnResetGameEvent()
    {
        RestartGame();
    }

    private void OnDrawFromStockEvent()
    {
        InputAction_PileClicked(PileKind.STOCK, -1);
    }

    private void OnGameWon()
    {
        UpdateWinsCount();
        _game.ResetSavedMoves();
        _ui.OnGameWon();
    }

    private void OnMenuOpened()
    {
        _input.Pause(true);
    }

    private void OnMenuClosed()
    {
        _input.Pause(false);
    }
    #endregion
}
