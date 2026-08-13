using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

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

    #region UserInteraction
    public virtual List<PileKind> GameAction_TrySmartMoveCard(Card card)
    {
        Log($"USER GameAction_TrySmartMoveCard {card}");
        List<PileKind> affectedPiles = new List<PileKind>();
        PileData sourcePileData = State.GetCardPileOwnerData(card);
        PileKind targetPileKind = default;

        ExecuteAction(() =>
        {
            GameCommand command = default;
            if (IsSafeToMoveCardToFoundation(card))
            {
                command = CommonInner_TryMoveCardToAnyFoundation(card, sourcePileData);
            }

            if (command is { Success: true })
            {
                targetPileKind = PileKind.FOUNDATION;
            }
            else
            {
                command = CommonInner_TryMoveCardToAnyTableau(card, sourcePileData);

                if (command is { Success: true })
                {
                    targetPileKind = PileKind.TABLEAU;
                }
                else
                {
                    if (sourcePileData.Kind != PileKind.FREECELL)
                    {
                        command = CommonInner_TryMoveCardToAnyFreeCell(card, sourcePileData);
                    }

                    if (command is { Success: true })
                    {
                        targetPileKind = PileKind.FREECELL;
                    }
                    else
                    {
                        command = CommonInner_TryMoveCardToAnyFoundation(card, sourcePileData);
                        if (command is { Success: true })
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                    }
                }
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
    #endregion

    #region AutomaticActions
    public List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card)
    {
        Log($"USER AutoAction_TryMoveCardToFoundationAutomatically {card}");
        List<PileKind> affectedPiles = new List<PileKind>();
        PileData sourcePileData = State.GetCardPileOwnerData(card);

        if (sourcePileData.Kind == PileKind.FOUNDATION || sourcePileData.Kind == PileKind.STOCK)
            return new List<PileKind>();

        ExecuteAction(() =>
        {
            GameCommand command = CommonInner_TryMoveCardToAnyFoundation(card, sourcePileData);

            if (command != null && command.Actions.Count > 0)
            {
                affectedPiles.Add(sourcePileData.Kind);
                affectedPiles.Add(PileKind.FOUNDATION);
            }
            return command;
        });

        return affectedPiles;
    }
    #endregion

    #region CommonActions
    public virtual List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex)
    {
        if (pileKind != PileKind.STOCK)
            return new List<PileKind>();

        return CommonGameAction_DrawFromStockOrRestock();
    }

    public List<PileKind> CommonGameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
    {
        Log($"USER CommonGameAction_TryMoveCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
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
    #endregion

    #region InnerActions
    protected GameCommand CommonInner_TryMoveCardToAnyFoundation(Card card, PileData sourcePileData)
    {
        int excludeIndex = -99;
        if (sourcePileData.Kind == PileKind.FOUNDATION)
        {
            excludeIndex = sourcePileData.Index;
            Log($"INNER CommonInner_TryMoveCardToAnyFoundation {card} (except to F[{excludeIndex}])");
        } else
        {
            Log($"INNER CommonInner_TryMoveCardToAnyFoundation {card} > F*");
        }

        GameCommand command = default;
        for (int i = 0; i < State.Foundations.Length; i++)
        {
            if (i == excludeIndex) continue;

            command = CommonInner_TryMoveCardToFoundationIndex(card, sourcePileData, i);
            if (command is { Success: true })
            {
                break;
            }
        }

        return command;
    }


    private GameCommand CommonInner_TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
    {
        Log($"INNER CommonInner_TryMoveCardToFoundationIndex {card} > F[{targetPileIndex}]");
        Queue<GameCommandAction> commandActions = new();

        if (CanAddCardToPile(card, PileKind.FOUNDATION, targetPileIndex))
        {
            GameCommandAction a = new GameCommandActionMove(
                sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                targetPile: PileKind.FOUNDATION, targetIndex: targetPileIndex
            );
            a.Execute(State);
            commandActions.Enqueue(a);

            if (!State.FoundationCardsFree)
            {
                a = new GameCommandActionFree(
                    card,
                    cardFreed: FreedAction.LOCKED
                );
                a.Execute(State);
                commandActions.Enqueue(a);
            }

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

    protected GameCommand CommonInner_TryMoveCardToAnyFreeCell(Card card, PileData sourcePileData)
    {
        int excludeIndex = -99;
        if (sourcePileData.Kind == PileKind.FREECELL)
        {
            excludeIndex = sourcePileData.Index;
            Log($"INNER CommonInner_TryMoveCardToAnyFreeCell {card} (except to FC[{excludeIndex}])");
        }
        else
        {
            Log($"INNER CommonInner_TryMoveCardToAnyFreeCell {card} > FC*");
        }
                Log($"INNER TryMoveCardToAnyFreeCell {card} > FC*");
        GameCommand command = default;
        for (int i = 0; i < State.FreeCells.Length; i++)
        {
            if (i == excludeIndex) continue;

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
        Log($"INNER CommonInner_TryMoveCardToFreeCellIndex{card} > FC[{targetPileIndex}]");
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
                commandActions.Enqueue(extraMoves.Dequeue());
            }
        }
        return new GameCommand(commandActions);
    }


    protected GameCommand CommonInner_TryMoveCardToAnyTableau(Card card, PileData sourcePileData)
    {
        int excludeIndex = -99;
        if (sourcePileData.Kind == PileKind.TABLEAU)
        {
            excludeIndex = sourcePileData.Index;
            Log($"INNER CommonInner_TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
        }
        else
        {
            Log($"INNER CommonInner_TryMoveCardToAnyTableau {card} > T*");
        }

        GameCommand command = default;

        List<int> emptyCandidates = new();
        List<int> compatibleFullCandidates = new();

        for (int i = 0; i < State.Tableaus.Length; i++)
        {
            if (i == excludeIndex) continue;

            if (CanAddCardToPile(card, PileKind.TABLEAU, i))
            {
                if (State.Tableaus[i].Count > 0)
                {
                    compatibleFullCandidates.Add(i);
                }
                else
                {
                    emptyCandidates.Add(i);
                }
            }
        }

        for (int i = 0; i < compatibleFullCandidates.Count; i++)
        {
            command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, compatibleFullCandidates[i]);
            if (command is { Success: true })
            {
                break;
            }
        }

        if (command == null || !command.Success)
        {
            for (int i = 0; i < emptyCandidates.Count; i++)
            {
                command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, emptyCandidates[i]);
                if (command is { Success: true })
                {
                    break;
                }
            }
        }

        return command;
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
                case PileKind.FREECELL:
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

    protected Queue<GameCommandAction> CommonInner_UpdateFreeCards()
    {
        Queue<GameCommandAction> commands = new();
        for (int i = 0; i < TableausAmount; i++)
        {
            for (int j = 0; j < State.Tableaus[i].Count; j++)
            {
                if (State.Tableaus[i].ElementAt(j).Free) continue;
                if (!State.Tableaus[i].ElementAt(j).Revealed) continue;
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

    #region Checks
    private bool IsSafeToMoveCardToFoundation(Card card)
    {
        if (card.Value < 3) return true;
        CardSuit[] oppositeColorSuites = CardUtils.GetOppositeColorSuits(card.Suit);

        bool card1Found = false;
        foreach (Foundation f in State.Foundations)
        {
            if (f.Suit == oppositeColorSuites[0])
            {
                foreach (Card c in f.Stack)
                {
                    if (c.Value == card.Value - 2)
                    {
                        card1Found = true;
                        break;
                    }
                }
            }
        }
        if (!card1Found) return false;

        bool card2found = false;
        foreach (Foundation f in State.Foundations)
        {
            if (f.Suit == oppositeColorSuites[1])
            {
                foreach (Card c in f.Stack)
                {
                    if (c.Value == card.Value - 2)
                    {
                        card2found = true;
                        break;
                    }
                }
            }
        }
        return card2found;
    }

    private bool CanMoveCardToAnyFoundation(Card card)
    {
        Log($"INNER CanMoveCardToAnyFoundation {card} > F*");
        if (card.Value == 1) return true;

        for (int i = 0; i < State.Foundations.Length; i++)
        {
            if (State.Foundations[i].Suit != card.Suit)
                continue;

            return CanAddCardToPile(card, PileKind.FOUNDATION, i);
        }
        return false;
    }
    #endregion

    #region Utilities

    public Card GetSolvableCard()
    {
        Debug.Log("Looking for automatic move");

        Card card;
        if (HasWaste)
        {
            State.WastePile.TryPeek(out card);

            if (card != null && IsSafeToMoveCardToFoundation(card))
            {
                Debug.Log($"Safe to move {card} to foundation. Can move?");
                if (CanMoveCardToAnyFoundation(card))
                {
                    Debug.Log($"{card} can be moved from waste to foundation.");
                    return card;
                }
            }
        }

        for (int i = 0; i < FreeCellsAmount; i++)
        {
            State.FreeCells[i].TryPeek(out card);
            if (card != null && IsSafeToMoveCardToFoundation(card))
            {
                Debug.Log($"Safe to move {card} to foundation. Can move?");
                if (CanMoveCardToAnyFoundation(card))
                {
                    Debug.Log($"{card} can be moved from free cell[{i}] to foundation.");

                    return card;
                }
            }
        }

        for (int i = 0; i < TableausAmount; i++)
        {
            State.Tableaus[i].TryPeek(out card);
            if (card != null && IsSafeToMoveCardToFoundation(card))
            {
                Debug.Log($"Safe to move {card} to foundation. Can move?");
                if (CanMoveCardToAnyFoundation(card))
                {
                    Debug.Log($"{card} can be moved from tableau[{i}] to foundation.");

                    return card;
                }
            }
        }

        return null;
    }
    #endregion
}
