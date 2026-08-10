using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public abstract class Game
{
    public abstract GameState State { get; }
    protected abstract string DebugTag { get; }


    protected abstract bool Won();

    public void Init()
    {
        ResetSavedMoves();
    }

    public void ResetSavedMoves()
    {
        State.ResetSavedMoves();
    }

    /// <summary>
    /// Command Wrapper!
    /// </summary>
    /// <param name="command"></param>
    /// <returns>Whether a UI refresh is needed</returns>
    protected bool ExecuteAction(Func<GameCommand> action)
    {
        GameCommand command = action();
        bool movesMade = command != null && command.Actions.Count > 0;

        if (movesMade)
        {
            State.SaveCommand(command);
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


    public virtual bool RedoCommmand()
    {
        bool redone = State.RedoCommand();
        if (redone)
            Log();
        return redone;
    }

    public virtual bool UndoCommand()
    {
        bool undone = State.UndoCommand();
        if (undone)
            Log();
        return undone;
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
