using System;
using System.Collections.Generic;
using System.Linq;
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
        return total == 13 * FoundationsAmount;
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
    public abstract List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex);
    public abstract Card GetSolvableCard();

    protected abstract bool ValidTableauCardStack(Card child, Card parent);

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
    public List<PileKind> CommonGameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
    {
        Log($"USER Action_DragCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
        List<PileKind> affectedPiles = new List<PileKind>();
        PileData sourcePileData = State.GetCardPileOwnerData(card);

        ExecuteAction(() =>
        {
            GameCommand command = default;
            switch (targetPileKind)
            {
                case PileKind.FOUNDATION:
                    command = CommonInner_TryMoveCardToFoundationIndex(card, sourcePileData, targetPileIndex);
                    break;
                case PileKind.TABLEAU:
                    command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, targetPileIndex);
                    break;
                case PileKind.FREECELL:
                    command = CommonInner_TryMoveCardToAnyFreeCell(card, sourcePileData);
                    break;
            }

            if (command is { Success: true })
            {
                affectedPiles.Add(sourcePileData.Kind);
                if (sourcePileData.Kind != targetPileKind)
                    affectedPiles.Add(targetPileKind);
            }
            return command;
        });

        return affectedPiles;
    }

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

    private GameCommand CommonInner_TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
    {
        Log($"INNER TryMoveCardToFoundationIndex {card} > F[{targetPileIndex}]");
        Queue<GameCommandAction> commandActions = new();

        if (CanAddCardToPile(card, PileKind.FOUNDATION, targetPileIndex))
        {
            GameCommandAction a = new GameCommandActionMove(
                sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                targetPile: PileKind.FOUNDATION, targetIndex: targetPileIndex
            );
            a.Execute(State);
            commandActions.Enqueue(a);

            a = new GameCommandActionFree(
                card,
                cardFreed: FreedAction.LOCKED
            );
            a.Execute(State);
            commandActions.Enqueue(a);

            Queue<GameCommandAction> extraMoves = CommonInner_AfterRemovingCardFromPile(sourcePileData);
            while (extraMoves.Count > 0)
            {
                a = extraMoves.Dequeue();
                a.Execute(State);
                commandActions.Enqueue(a);
            }
        }
        return new GameCommand(commandActions);
    }

    private GameCommand CommonInner_TryMoveCardToAnyFreeCell(Card card, PileData sourcePileData)
    {
        Log($"INNER TryMoveCardToAnyFreeCell {card} > FC*");
        GameCommand command = default;
        for (int i = 0; i < State.FreeCells.Length; i++)
        {
            if (State.FreeCells[i].Count > 0)
                continue;

            command = CommonInner_TryMoveCardToFreeCellIndex(card, sourcePileData, i);
            if (command is { Success: true })
            {
                break;
            }
        }
        return command;
    }

    private GameCommand CommonInner_TryMoveCardToFreeCellIndex(Card card, PileData sourcePileData, int targetPileIndex)
    {
        Log($"INNER TryMoveCardToFreeCellIndex{card} > FC[{targetPileIndex}]");
        Queue<GameCommandAction> commandActions = new();
        if (CanAddCardToPile(card, PileKind.FREECELL, targetPileIndex))
        {
            GameCommandAction a = new GameCommandActionMove(
                 sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                 targetPile: PileKind.FREECELL, targetIndex: targetPileIndex
             );
            a.Execute(State);
            commandActions.Enqueue(a);

            Queue<GameCommandAction> extraMoves = CommonInner_AfterRemovingCardFromPile(sourcePileData);
            while (extraMoves.Count > 0)
            {
                a = extraMoves.Dequeue();
                a.Execute(State);
                commandActions.Enqueue(a);
            }
        }
        return new GameCommand(commandActions);
    }

    private GameCommand CommonInner_TryMoveCardsToTableauIndex(Card card, PileData sourcePileData, int targetPileIndex)
    {
        Log($"INNER TryMoveCardsToTableauIndex {card} > T[{targetPileIndex}]");
        Queue<GameCommandAction> commandActions = new();

        if (CanAddCardToPile(card, PileKind.TABLEAU, targetPileIndex)) //VALIDATION DONE
        {
            switch (sourcePileData.Kind)
            {
                case PileKind.TABLEAU:
                    GameCommandAction moveStackAction = new GameCommandActionMoveStack(
                        card,
                        sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                        targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                    );
                    moveStackAction.Execute(State);
                    commandActions.Enqueue(moveStackAction);
                    break;
                case PileKind.FOUNDATION:
                case PileKind.WASTE:
                    GameCommandAction moveAction = new GameCommandActionMove(
                               sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                               targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                           );
                    moveAction.Execute(State);
                    commandActions.Enqueue(moveAction);
                    break;
            }
            Queue<GameCommandAction> extraCommands = CommonInner_AfterRemovingCardFromPile(sourcePileData);
            while (extraCommands.Count > 0)
            {
                commandActions.Enqueue(extraCommands.Dequeue());
            }
        }
        return new GameCommand(commandActions);
    }

    protected Queue<GameCommandAction> CommonInner_AfterRemovingCardFromPile(PileData pileData)
    {
        Log($"INNER RemovedCardFromPile {pileData.Kind}[{pileData.Index}]");
        Queue<GameCommandAction> result = new();

        switch (pileData.Kind)
        {
            case PileKind.WASTE:
                if (State.WastePile.Count > 0)
                {
                    GameCommandAction a = new GameCommandActionFree(
                        State.WastePile.Peek(),
                        cardFreed: FreedAction.FREED
                    );
                    a.Execute(State);
                    result.Enqueue(a);
                }
                break;
            case PileKind.FOUNDATION:
                if (State.FoundationCardsFree && State.Foundations[pileData.Index].Stack.Count > 0)
                {
                    GameCommandAction a = new GameCommandActionFree(
                       State.Foundations[pileData.Index].Stack.Peek(),
                        cardFreed: FreedAction.FREED
                    );
                    a.Execute(State);
                    result.Enqueue(a);
                }
                break;
            case PileKind.TABLEAU:
                if (State.Tableaus[pileData.Index].Count > 0)
                {
                    Card tCard = State.Tableaus[pileData.Index].Peek();
                    if (!tCard.Revealed)
                    {
                        GameCommandAction a = new GameCommandActionReveal(
                            tCard,
                            cardRevealed: RevealedAction.REVEALED);
                        a.Execute(State);
                        result.Enqueue(a);
                    }
                    if (!tCard.Free)
                    {
                        GameCommandAction a = new GameCommandActionFree(
                            tCard,
                            cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        result.Enqueue(a);
                    }
                }
                Queue<GameCommandAction> extraCommands = CommonInner_UpdateFreeCards();
                while (extraCommands.Count > 0)
                {
                    result.Enqueue(extraCommands.Dequeue());
                }
                break;
        }
        return result;
    }

    private Queue<GameCommandAction> CommonInner_UpdateFreeCards()
    {
        Queue<GameCommandAction> commands = new();
        for (int i = 0; i < TableausAmount; i++)
        {
            for (int j = 0; j < State.Tableaus[i].Count; j++)
            {
                if (State.Tableaus[i].ElementAt(j).Free) continue;
                if (j == 0)
                {
                    GameCommandAction a = new GameCommandActionFree(
                         State.Tableaus[i].ElementAt(j),
                         cardFreed: FreedAction.FREED);
                    a.Execute(State);
                    commands.Enqueue(a);
                }
                if (j > 0)
                {
                    if (ValidTableauCardStack(State.Tableaus[i].ElementAt(j - 1), State.Tableaus[i].ElementAt(j)))
                    {
                        GameCommandAction a = new GameCommandActionFree(
                            State.Tableaus[i].ElementAt(j),
                            cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        commands.Enqueue(a);
                    }
                    else break;
                }
            }
        }
        return commands;
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
