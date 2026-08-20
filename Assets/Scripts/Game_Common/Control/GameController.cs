using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;

public abstract class GameController : MonoBehaviour
{
    [SerializeField] protected MyInputManager _input;
    [SerializeField] protected GameUI _ui;
    [SerializeField] protected GameView _gameView;

    private const string GAME_BUSY_REASON = "IBR_GameProcessing";
    private const string MENU_OPENED_REASON = "IBR_MenuOpened";

    public GameStatus Status
    {
        get => _status;
        private set
        {
            _status = value;
            Debug.Log($"STATUS {value}");
        }
    }

    protected List<string> _viewsRefreshing = new();
    protected List<Card> _deck;
    protected Game _game;
    private GameStatus _status = GameStatus.INITIALIZING;
    private static List<PileKind> AllPileKinds = new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU, PileKind.WASTE, PileKind.FOUNDATION, PileKind.FREECELL };
    private bool IsBusy => _status != GameStatus.LISTENING;

    #region Initialization
    void Start()
    {
        Status = GameStatus.INITIALIZING;
        _input.BlockInput(GAME_BUSY_REASON);
        Initialize(() =>
        {
            StartNewGame(() =>
            {
                Listening();
            });
        });
    }

    private void Initialize(Action onDone)
    {
        LoadDeck();
        RegisterToEvents();
        InitConfig();
        _ui.Init(this);
        _gameView.Init(this);
        LoadDeckView(onDone);
    }

    private void StartNewGame(Action onDone)
    {
        ResetDeck();
        ResetView();
        LoadGame();
        _game.Init();
        CheckRefreshView(new List<PileKind> { PileKind.STOCK, PileKind.TABLEAU, PileKind.WASTE, PileKind.FOUNDATION }, onDone, immediate: true);
    }

    private void ResetDeck()
    {
        foreach (Card card in _deck)
        {
            card.Show(false);
            card.FreeCard(false);
        }
    }

    private void Processing()
    {
        Status = GameStatus.PROCESSING;
        _input.BlockInput(GAME_BUSY_REASON);
    }

    private void Listening()
    {
        Status = GameStatus.LISTENING;
        _input.UnblockInput(GAME_BUSY_REASON);
    }

    void OnDestroy()
    {
        DeregisterFromEvents();
    }
    #endregion

    #region GameController
    public abstract bool IsRestockAvailable();
    protected abstract void LoadDeck();
    protected abstract bool IsAutoMovesEnabled();
    protected abstract void UpdateWinsCount();
    protected abstract void LoadGame();
    protected virtual void InitConfig() { }
    protected virtual void ResetSettingsToDefault() { }

    public bool InputAction_CardDraggedToPile(Card card, PileKind targetPileKind, int targetPileIndex, Vector3 originalCardPosition)
    {
        Debug.Log("Processing card dragged to pile.");
        if (IsBusy) return false;
        Processing();

        PileData sourcePileData = _game.State.GetCardPileOwnerData(card);
        bool shouldHoldAutoMoves = sourcePileData.Kind == PileKind.FOUNDATION;

        return CheckRefreshView(
            _game.CommonGameAction_TryMoveCardToPile(card, sourcePileData, targetPileKind, targetPileIndex), () =>
            {
                Listening();
            }, card, originalCardPosition, immediate: true, holdAutoMoves: shouldHoldAutoMoves);
    }

    public bool InputAction_CardDoubleClicked(Card card, Vector3 originalCardPosition)
    {
        Debug.Log("Processing double click.");
        if (IsBusy) return false;
        Processing();

        PileData sourcePileData = _game.State.GetCardPileOwnerData(card);
        bool shouldHoldAutoMoves = sourcePileData.Kind == PileKind.FOUNDATION;

        return CheckRefreshView(
            _game.GameAction_TrySmartMoveCard(card, sourcePileData), () =>
            {
                Listening();
            }, card, originalCardPosition, holdAutoMoves: shouldHoldAutoMoves);
    }

    public bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
    {
        return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
    }

    public void RestartGame()
    {
        if (IsBusy) return;
        Processing();
        StartNewGame(() =>
        {
            Listening();
            EventManager.OnGameStarted?.Invoke();
        });
    }

    public bool InputAction_PileClicked(PileKind pileKind, int pileIndex)
    {
        Debug.Log("Processing pile clicked.");
        if (IsBusy) return false;
        Processing();

        return CheckRefreshView(
            _game.GameAction_ClickedPile(pileKind, pileIndex), () =>
            {
                Listening();
            });
    }

    protected List<PileKind> AutoAction_MoveCardAutomatically(Card card)
    {
        return _game.AutoAction_TryMoveCardToFoundationAutomatically(card);
    }
    #endregion

    #region View
    protected void ResetView() {
        _gameView.Reset();
    }

    protected void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false)
    {
        Debug.Log("RefreshViewTask.");

        foreach (PileKind kind in pilesToRefresh)
        {
          //  string tag = kind.ToString();
            switch (kind)
            {
                case PileKind.WASTE:
                    if (!_game.HasWaste) break;
                    _viewsRefreshing.Add(kind.ToString());
                    _gameView.RefreshWaste(
                        CardUtils.CloneCardPile(_game.State.WastePile),
                        cardMoved,
                        originalCardPosition,
                        immediate,
                        () => {_viewsRefreshing.Remove(kind.ToString()); });
                    break;

                case PileKind.STOCK:
                    if (!_game.HasStock) break;
                    _viewsRefreshing.Add(kind.ToString());
                    _gameView.RefreshStock(
                        CardUtils.CloneCardPile(_game.State.StockPile),
                        cardMoved,
                        originalCardPosition,
                        immediate,
                        () => { _viewsRefreshing.Remove(kind.ToString()); });
                    break;

                case PileKind.FOUNDATION:
                    if (_game.FoundationsAmount == 0) break;
                    _viewsRefreshing.Add(kind.ToString());
                    _gameView.RefreshFoundations(
                        CardUtils.GetClonedCardPiles(_game.State.Foundations),
                        cardMoved,
                        originalCardPosition,
                        immediate,
                        () => { _viewsRefreshing.Remove(kind.ToString()); });
                    break;

                case PileKind.TABLEAU:
                    if (_game.TableausAmount == 0) break;
                    _viewsRefreshing.Add(kind.ToString());
                    _gameView.RefreshTableaus(
                        CardUtils.GetClonedCardPiles(_game.State.Tableaus),
                        cardMoved,
                        originalCardPosition,
                        immediate,
                        () => { _viewsRefreshing.Remove(kind.ToString()); });
                    break;

                case PileKind.FREECELL:
                    if (_game.FreeCellsAmount == 0) break;
                    _viewsRefreshing.Add(kind.ToString());
                    _gameView.RefreshFreeCells(
                        CardUtils.GetClonedCardPiles(_game.State.FreeCells),
                        cardMoved,
                        originalCardPosition,
                        immediate,
                        () => { _viewsRefreshing.Remove(kind.ToString()); });
                    break;
            }
        }
        Debug.Log("Await...");
        StartCoroutine(WaitForViewsToBeRefreshed(onDone));
    }

    private IEnumerator WaitForViewsToBeRefreshed(Action onDone)
    {
        while (_viewsRefreshing.Count > 0) yield return null;
        Debug.Log("Done.");
        onDone?.Invoke();
    }

    public bool IsCardInTargetPile(Card card, out TargetCardPileView result)
    {
        return _gameView.IsCardInATargetablePile(_game.State.GetCardPileOwnerData(card), out result);
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

    private bool CheckRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false, bool holdAutoMoves = false)
    {
        Debug.Log("RefreshView.");
        bool refreshNeeded = pilesToRefresh.Count > 0;
        if (refreshNeeded)
        {
            _ui.UpdateUndoRedoButtons(enableUndoBtn:_game.State.UndoAvailable,enableRedoBtn:_game.State.RedoAvailable);
            DoRefreshView(pilesToRefresh, () =>
            {
                if (!holdAutoMoves && IsAutoMovesEnabled())
                {
                    Card solvableCard = _game.GetSolvableCard();
                    if (solvableCard == null)
                    {
                        Debug.Log("No auto moves availables.");
                        onDone?.Invoke();
                        return;
                    }

                    Vector3 originalSolvableCardPosition = GetCardViewPosition(solvableCard);
                    Debug.Log("Auto moves enabled. Solving automatic move.");
                    CheckRefreshView(
                        AutoAction_MoveCardAutomatically(solvableCard), onDone, solvableCard, originalSolvableCardPosition);
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
        EventManager.OnResetGameRequested += OnResetGameEvent;
        EventManager.OnDrawFromStock += OnDrawFromStockEvent;
        EventManager.OnGameWon += OnGameWon;
        EventManager.OnMenuClosed += OnMenuClosed;
        EventManager.OnMenuOpened += OnMenuOpened;
        EventManager.OnUndo += OnUndo;
        EventManager.OnRedo += OnRedo;
    }

    private void DeregisterFromEvents()
    {
        EventManager.OnResetGameRequested -= OnResetGameEvent;
        EventManager.OnDrawFromStock -= OnDrawFromStockEvent;
        EventManager.OnGameWon -= OnGameWon;
        EventManager.OnMenuClosed -= OnMenuClosed;
        EventManager.OnMenuOpened -= OnMenuOpened;
        EventManager.OnUndo -= OnUndo;
        EventManager.OnRedo -= OnRedo;
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
        _input.BlockInput(MENU_OPENED_REASON);
    }

    private void OnMenuClosed()
    {
        _input.UnblockInput(MENU_OPENED_REASON);
    }

    private void OnUndo()
    {
        Debug.Log("Processing UNDO.");
        if (IsBusy) return;
        Processing();

        bool success = _game != null && _game.UndoCommand();

        if (success)
        {
            DoRefreshView(AllPileKinds, () =>
            {
                Listening();
            }, null, default, true);
            _ui.UpdateUndoRedoButtons(enableUndoBtn: _game.State.UndoAvailable, enableRedoBtn: _game.State.RedoAvailable);
        }
        else
        {
            Listening();
        }
    }

    private void OnRedo()
    {
        Debug.Log("Processing REDO.");
        if (IsBusy) return;
        Processing();

        bool success = _game != null && _game.RedoCommmand();

        if (success)
        {
            DoRefreshView(AllPileKinds, () =>
            {
                Listening();
            }, null, default, true);
            _ui.UpdateUndoRedoButtons(enableUndoBtn: _game.State.UndoAvailable, enableRedoBtn: _game.State.RedoAvailable);
        }
        else
        {
            Listening();
        }
    }
    #endregion
}
