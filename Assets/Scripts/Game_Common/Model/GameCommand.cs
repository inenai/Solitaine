using System.Collections.Generic;

namespace Common
{
    public class GameCommand
    {
        public Queue<GameCommandAction> Actions => _actions;
        private Queue<GameCommandAction> _actions;
        public bool Success => _actions != null && _actions.Count > 0;

        public GameCommand(GameCommandAction action)
        {
            _actions = new();
            _actions.Enqueue(action);
        }

        public GameCommand(Queue<GameCommandAction> actions)
        {
            _actions = actions;
        }

        public void Execute(GameState s)
        {
            foreach (GameCommandAction a in _actions)
            {
                a.Execute(s);
            }
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

            Stack<Card> source = s.GetCardStack(sourceKind, sourceIndex);
            Stack<Card> target = s.GetCardStack(targetKind, targetIndex);

            Card card = source.Pop();

            if (sourceKind == PileKind.FOUNDATION && source.Count == 0)
            {
                s.Foundations[sourceIndex].Suit = null;
            }

            if (targetKind == PileKind.FOUNDATION && target.Count == 0)
            {
                s.Foundations[targetIndex].Suit = card.Suit;
            }

            target.Push(card);
        }
    }

    public class GameCommandActionFundationSuit : GameCommandAction
    {
        private CardSuit _suit;
        private int _foundationIndex;

        public GameCommandActionFundationSuit(CardSuit suit, int foundationIndex)
        {
            _suit = suit;
            _foundationIndex = foundationIndex;
        }
        public override void Execute(GameState s, bool undo)
        {
            s.Foundations[_foundationIndex].Suit = undo ? null : _suit;
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
            Stack<Card> source = undo ? s.GetCardStack(PileKind.STOCK) : s.GetCardStack(PileKind.WASTE);
            Stack<Card> target = undo ? s.GetCardStack(PileKind.WASTE) : s.GetCardStack(PileKind.STOCK);

            while (source.Count > 0)
            {
                Card card = source.Pop();
                card.Show(undo);
                target.Push(card);
            }

            s.OnRestock(undo);
        }
    }
}