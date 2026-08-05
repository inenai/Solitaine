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
            ExecuteAndSaveCommand(command);
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

    protected virtual void ExecuteCommandAction(GameCommandAction command, bool undo = false)
    {
        switch (command.CardRevealed)
        {
            case RevealedAction.REVEALED:
                command.Card.Show(!undo);
                break;
            case RevealedAction.HID:
                command.Card.Show(undo);
                break;
        }

        switch (command.CardFreed)
        {
            case FreedAction.FREED:
                command.Card.FreeCard(!undo);
                break;
            case FreedAction.LOCKED:
                command.Card.FreeCard(undo);
                break;
        }

        if (command.Moved)
        {
            PileKind sourceKind = command.SourcePile.Value;
            int sourceIndex = command.SourceIndex.Value;

            PileKind targetKind = command.TargetPile.Value;
            int targetIndex = command.TargetIndex.Value;

            if (undo)
            {
                sourceKind = command.TargetPile.Value;
                sourceIndex = command.TargetIndex.Value;

                targetKind = command.SourcePile.Value;
                targetIndex = command.SourceIndex.Value;
            }

            Stack<Card> source = State.GetCardStack(sourceKind, sourceIndex);
            Stack<Card> target = State.GetCardStack(targetKind, targetIndex);

            Card card = source.Pop();

            if (sourceKind == PileKind.FOUNDATION && source.Count == 0)
            {
                State.Foundations[sourceIndex].Suit = null;
            }

            if (targetKind == PileKind.FOUNDATION && target.Count == 0)
            {
                State.Foundations[targetIndex].Suit = card.Suit;
            }

            target.Push(card);
        }
    }

    protected virtual void ExecuteAndSaveCommand(GameCommand command)
    {
        foreach (GameCommandAction a in command.Actions)
        {
            ExecuteCommandAction(a);
        }

        _undoneMoves?.Clear();
        _doneMoves.Push(command);
    }

    protected virtual void RedoCommmand()
    {
        GameCommand c = _undoneMoves.Pop();
        foreach (GameCommandAction a in c.Actions)
            ExecuteCommandAction(a);
        _doneMoves.Push(c);
    }

    protected virtual void UndoCommand()
    {
        GameCommand c = _undoneMoves.Pop();
        foreach (GameCommandAction a in c.Actions)
            ExecuteCommandAction(a, undo:true);
        _doneMoves.Push(c);
    }

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
