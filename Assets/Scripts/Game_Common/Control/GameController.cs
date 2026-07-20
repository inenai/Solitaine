using System;
using System.Collections.Generic;
using Common;
using TMPro.EditorUtilities;
using UnityEngine;

public abstract class GameController : MonoBehaviour, IGameController
{
    [SerializeField] protected MyInputManager _input;
    [SerializeField] protected GameUI _ui;


    protected List<Card> _deck;
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
    public abstract bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex);
    public abstract bool IsCardInTargetPile(Card card, out TargetCardPileView result);
    protected abstract void InitView();
    protected abstract void CreateDeck();
    protected abstract void CreateGame();
    protected abstract void ResetView();
    protected abstract void InitConfig();
    protected abstract void UpdateWinsCount();
    protected abstract Card GetSolvableCard();
    protected abstract Vector3 GetCardViewPosition(Card card);
    protected abstract void LoadDeckView(Action onDone);
    protected abstract void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false);

    protected abstract Func<List<PileKind>> Action_PileClicked(PileKind kind);
    protected abstract List<PileKind> Action_DoubleClickedCard(Card card);
    protected abstract List<PileKind> Action_DragCardToPile(Card card, PileKind targetPileKind, int targetPileIndex);
    protected abstract List<PileKind> Auto_MoveCardAutomatically(Card card);

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
        InitView();
        LoadDeckView(onDone);
    }

    private void StartNewGame(Action onDone)
    {
        _ui.OnStartNewGame();
        ResetDeck();
        ResetView();
        CreateGame();
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

    public bool PileClicked(PileKind pileKind, int pileIndex)
    {
        Debug.Log("Processing pile clicked.");
        if (Status != GameStatus.LISTENING) return false;
        Status = GameStatus.PROCESSING;

        Func<List<PileKind>> action = Action_PileClicked(pileKind);

        if (action == null)
        {
            Status = GameStatus.LISTENING;
            return false;
        }

        return CheckRefreshView(
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

        return CheckRefreshView(
            Action_DoubleClickedCard(card), card, originalCardPosition, () =>
            {
                Status = GameStatus.LISTENING;
            });
    }

    public bool CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex, Vector3 originalCardPosition)
    {
        Debug.Log("Processing card dragged to pile.");
        if (Status != GameStatus.LISTENING) return false;
        Status = GameStatus.PROCESSING;

        return CheckRefreshView(
            Action_DragCardToPile(card, targetPileKind, targetPileIndex), card, originalCardPosition, () =>
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
                        Auto_MoveCardAutomatically(solvableCard), solvableCard, originalSolvableCardPosition, onDone, false);
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
        PileClicked(PileKind.STOCK, -1);
    }

    private void OnGameWon()
    {
        UpdateWinsCount();
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
