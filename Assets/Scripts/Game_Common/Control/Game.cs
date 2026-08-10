using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

public abstract class Game
{
    public abstract GameState State { get; }
    protected abstract string DebugTag { get; }
    protected abstract int DrawCount { get; }
    public abstract int FoundationsAmount { get; }
    public abstract int TableausAmount { get; }
    public abstract int FreeCellsAmount { get; }
    public abstract bool HasStock { get; }
    public abstract bool HasWaste { get; }

    protected virtual bool Won()
    {
        int total = 0;
        foreach (Foundation f in State.Foundations)
        {
            total += f.Stack.Count;
        }
        return total == 13 * 4;
    }

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

    public abstract bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex);
    public abstract List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card);
    public abstract List<PileKind> GameAction_TrySmartMoveCard(Card card);
    public abstract List<PileKind> GameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex);
    public abstract List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex);
    public abstract Card GetSolvableCard();

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
    #endregion



    #region CommonActions
    protected List<PileKind> CommonGameAction_DrawFromStockOrRestock()
    {
        List<PileKind> affectedPiles = new List<PileKind>();
        bool uiRefreshNeeded = ExecuteAction(() =>
        {
            GameCommand command = default;
            int drewAmount = 0;
            command = CommonInner_TryDrawCardsFromStock(out drewAmount);
            if (command is { Success: true })
            {
                return command;
            }
            else
            {
                command = CommonInner_TryRestock();
            }
            return command;
        });

        if (uiRefreshNeeded)
        {
            affectedPiles.Add(PileKind.WASTE);
            affectedPiles.Add(PileKind.STOCK);
        }
        return affectedPiles;
    }

    private GameCommand CommonInner_TryDrawCardsFromStock(out int drewAmount)
    {
        Log("INNER TryDrawCardsFromStock");
        drewAmount = 0;

        if (State.StockPile.Count == 0)
        {
            return null;
        }

        Queue<GameCommandAction> commands = new();

        if (State.WastePile.TryPeek(out var topCard))
        {
            GameCommandAction a = new GameCommandActionFree(
                topCard,
                cardFreed: FreedAction.LOCKED
            );
            a.Execute(State);
            commands.Enqueue(a);
        }

        int stockFinalAmount = Mathf.Max(0, State.StockPile.Count - DrawCount);
        for (int i = State.StockPile.Count - 1; i >= stockFinalAmount; i--)
        {
            GameCommandAction a = new GameCommandActionMove(
                   sourcePile: PileKind.STOCK,
                   targetPile: PileKind.WASTE
              );
            a.Execute(State);
            commands.Enqueue(a);

            a = new GameCommandActionReveal(
                State.WastePile.Peek(),
                cardRevealed: RevealedAction.REVEALED
            );
            a.Execute(State);
            commands.Enqueue(a);

            if (i == stockFinalAmount)
            {
                a = new GameCommandActionFree(
                  State.WastePile.Peek(),
                  cardFreed: FreedAction.FREED
               );
                a.Execute(State);
                commands.Enqueue(a);
            }
        }
        return new GameCommand(commands);
    }

    private GameCommand CommonInner_TryRestock()
    {
        Log("INNER AttemptRestock");

        Queue<GameCommandAction> commands = new();
        if (State.WastePile.Count > 0 && State.AvailableRestocks != 0)
        {
            GameCommandAction a = new GameCommandActionRestock();
            a.Execute(State);
            commands.Enqueue(a);
        }
        return new GameCommand(commands);
    }

    #endregion
}
