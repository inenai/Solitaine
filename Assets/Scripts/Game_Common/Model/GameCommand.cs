using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public class GameCommand
    {
        public Queue<GameCommandAction> Actions => _actions;
        private Queue<GameCommandAction> _actions;
        public bool Valid => _actions != null && _actions.Count > 0;

        public GameCommand(GameCommandAction action)
        {
            _actions = new();
            _actions.Enqueue(action);
        }

        public GameCommand(Queue<GameCommandAction> actions)
        {
            _actions = actions;
        }

        public void Enqueue(GameCommandAction gca)
        {
            _actions.Enqueue(gca);
        }
    }

    public abstract class GameCommandAction
    {
        public abstract void Execute(GameState s, bool undo = false);
    }

    public class GameCommandActionMove : GameCommandAction
    {
        private PileKind _sourcePile;
        private int _sourceIndex;
        private PileKind _targetPile;
        private int _targetIndex;

        public GameCommandActionMove(PileKind sourcePile, PileKind targetPile, int sourceIndex = -1, int targetIndex = -1)
        {
            _sourcePile = sourcePile;
            _sourceIndex = sourceIndex;
            _targetPile = targetPile;
            _targetIndex = targetIndex;
        }

        public override void Execute(GameState s, bool undo = false)
        {
            string message = undo ? "UNDOING " : "";
            Debug.Log($"[COMMAND] {message}Move from {_sourcePile}[{_sourceIndex}] to {_targetPile}[{_targetIndex}]");

            PileKind sourceKind = _sourcePile;
            int sourceIndex = _sourceIndex;

            PileKind targetKind = _targetPile;
            int targetIndex = _targetIndex;

            if (undo)
            {
                sourceKind = _targetPile;
                sourceIndex = _targetIndex;

                targetKind = _sourcePile;
                targetIndex = _sourceIndex;
            }

            CardPile source = s.GetCardStack(sourceKind, sourceIndex);
            CardPile target = s.GetCardStack(targetKind, targetIndex);

            Card card = source.Pop();

            target.Push(card);
        }
    }

    public class GameCommandActionReveal : GameCommandAction
    {
        private Card _card;
        private RevealedAction _revealed;

        public GameCommandActionReveal(Card card, RevealedAction cardRevealed)
        {
            _card = card;
            _revealed = cardRevealed;
        }

        public override void Execute(GameState s, bool undo)
        {
            string message = undo ? "UNDOING " : "";
            string revealMessage = _revealed == RevealedAction.REVEALED ? "Reveal" : _revealed == RevealedAction.HID ? "Hide" : "No change!!!";
            Debug.Log($"[COMMAND] {message}{revealMessage} card {_card}");
            switch (_revealed)
            {
                case RevealedAction.REVEALED:
                    _card.Show(!undo);
                    break;
                case RevealedAction.HID:
                    _card.Show(undo);
                    break;
            }
        }
    }

    public class GameCommandActionFree : GameCommandAction
    {
        private Card _card;
        private FreedAction _freed;

        public GameCommandActionFree(Card card, FreedAction cardFreed)
        {
            _card = card;
            _freed = cardFreed;
        }

        public override void Execute(GameState s, bool undo)
        {
            string message = undo ? "UNDOING " : "";
            string freedMessage = _freed == FreedAction.FREED ? "Free" : _freed == FreedAction.LOCKED ? "Lock" : "No change!!!";
            Debug.Log($"[COMMAND] {message}{freedMessage} card {_card}");
            switch (_freed)
            {
                case FreedAction.FREED:
                    _card.FreeCard(!undo);
                    break;
                case FreedAction.LOCKED:
                    _card.FreeCard(undo);
                    break;
            }
        }
    }

    public class GameCommandActionRestock : GameCommandAction
    {
        public override void Execute(GameState s, bool undo = false)
        {
            string message = undo ? "UNDOING " : "";
            Debug.Log($"[COMMAND] {message}Restock");
            CardPile source = undo ? s.GetCardStack(PileKind.STOCK) : s.GetCardStack(PileKind.WASTE);
            CardPile target = undo ? s.GetCardStack(PileKind.WASTE) : s.GetCardStack(PileKind.STOCK);

            while (source.Count > 0)
            {
                Card card = source.Pop();
                card.Show(undo);
                target.Push(card);
            }

            s.OnRestock(undo);
        }
    }

    public class GameCommandActionMoveStack : GameCommandAction
    {
        private Card _card;
        private PileKind _sourcePile;
        private int _sourceIndex;
        private PileKind _targetPile;
        private int _targetIndex;

        public GameCommandActionMoveStack(Card card, PileKind sourcePile, PileKind targetPile, int sourceIndex = -1, int targetIndex = -1)
        {
            _card = card;
            _sourcePile = sourcePile;
            _sourceIndex = sourceIndex;
            _targetPile = targetPile;
            _targetIndex = targetIndex;
        }

        public override void Execute(GameState s, bool undo = false)
        {
            string message = undo ? "UNDOING " : "";
            Debug.Log($"[COMMAND] {message}Move card {_card} stack from {_sourcePile}[{_sourceIndex}] to {_targetPile}[{_targetIndex}]");

            PileKind sourceKind = _sourcePile;
            int sourceIndex = _sourceIndex;

            PileKind targetKind = _targetPile;
            int targetIndex = _targetIndex;

            if (undo)
            {
                sourceKind = _targetPile;
                sourceIndex = _targetIndex;

                targetKind = _sourcePile;
                targetIndex = _sourceIndex;
            }

            CardPile source = s.GetCardStack(sourceKind, sourceIndex);
            CardPile target = s.GetCardStack(targetKind, targetIndex);
            CardPile tempStack = new();

            Card next = source.Pop();
            while (next != _card)
            {
                tempStack.Push(next);
                next = source.Pop();
            }
            tempStack.Push(next);

            while(tempStack.Count > 0)
            {
                target.Push(tempStack.Pop());
            }
        }
    }
}