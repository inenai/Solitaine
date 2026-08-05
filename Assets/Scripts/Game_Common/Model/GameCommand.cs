using System.Collections.Generic;

namespace Common
{
    public class GameCommand
    {
        public Queue<GameCommandAction> Actions => _actions;
        private Queue<GameCommandAction> _actions;

        public GameCommand(GameCommandAction action)
        {
            _actions = new();
            _actions.Enqueue(action);
        }

        public GameCommand(Queue<GameCommandAction> actions)
        {
            _actions = actions;
        }
    }

    public struct GameCommandAction
    {
        private Card _card;
        private RevealedAction _revealed;
        private FreedAction _freed;
        private bool _moved;
        private PileKind? _sourcePile;
        private int? _sourceIndex;
        private PileKind? _targetPile;
        private int? _targetIndex;

        public Card Card => _card;
        public bool Moved => _moved;
        public PileKind? SourcePile => _sourcePile;
        public PileKind? TargetPile => _targetPile;
        public int? SourceIndex => _sourceIndex;
        public int? TargetIndex => _targetIndex;

        public RevealedAction CardRevealed => _revealed;
        public FreedAction CardFreed => _freed;

        public GameCommandAction(Card card, RevealedAction cardRevealed = RevealedAction.NO_CHANGE, FreedAction cardFreed = FreedAction.NO_CHANGE, bool cardMoved = false, PileKind? sourcePile = null, int? sourceIndex = null, PileKind? targetPile = null, int? targetIndex = null)
        {
            _card = card;
            _moved = cardMoved;
            _sourcePile = sourcePile;
            _sourceIndex = sourceIndex;
            _targetPile = targetPile;
            _targetIndex = targetIndex;
            _revealed = cardRevealed;
            _freed = cardFreed;
        }
    }
}