using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public abstract class Game
{
    public abstract GameState State { get; }
    protected abstract string DebugTag { get; }

    Stack<GameCommand> _doneMoves;
    Stack<GameCommand> _undoneMoves;
    protected abstract bool Won();


    public void Init()
    {
        ResetSavedMoves();
    }

    public void ResetSavedMoves()
    {
        _doneMoves = new();
        _undoneMoves = new();
    }

    /// <summary>
    /// Command Wrapper!
    /// </summary>
    /// <param name="command"></param>
    /// <returns>Whether a UI refresh is needed</returns>
    protected bool ExecuteAction(Func<GameCommand> action)
    {
        if (_doneMoves == null) _doneMoves = new();

        GameCommand command = action();
        bool movesMade = command != null && command.Actions.Count > 0;

        if (movesMade)
        {
            SaveCommand(command);
        }

        if (movesMade)
        {
            Log();

            if (Won())
            {
                State.OnWin();
                EventManager.OnGameWon?.Invoke();
            }
        }

        return movesMade;
    }

    #region Commands

    protected virtual void SaveCommand(GameCommand command)
    {
        _undoneMoves?.Clear();
        _doneMoves.Push(command);
    }

    protected virtual void RedoCommmand()
    {
        GameCommand c = _undoneMoves.Pop();
        foreach (GameCommandAction a in c.Actions)
            a.Execute(State);
        _doneMoves.Push(c);
    }

    protected virtual void UndoCommand()
    {
        GameCommand c = _undoneMoves.Pop();
        foreach (GameCommandAction a in c.Actions)
            a.Execute(State, undo: true);
        _doneMoves.Push(c);
    }

    #endregion

    #region utils
    protected void Log(string message)
    {
        Debug.Log($"[{DebugTag}] {message}");
    }

    public void Log()
    {
        Debug.Log("=== SOLITAIRE STATE ===");
        State.LogState();
    }

    public abstract bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex);
    public abstract List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card);
    public abstract List<PileKind> GameAction_TrySmartMoveCard(Card card);
    public abstract List<PileKind> GameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex);
    public abstract List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex);
    public abstract Card GetSolvableCard();


    #endregion
}
