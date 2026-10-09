using Common;
using Storage;

namespace Klondike
{
    public class KlondikeState : GameState
    {
        public override int AvailableRestocks => _availableRestocks;
        public override bool FoundationCardsFree => true;
        public int DrawAmount => _drawAmount;
        public override SolitaireKind SolitaireKind => SolitaireKind.KLONDIKE;

        int _drawAmount = KlondikeSettings.DEFAULT_DRAW_AMOUNT;
        int _availableRestocks = KlondikeSettings.DEFAULT_RESTOCKS;

        public KlondikeState(Game game, SavedGame progress) : base(game, progress)
        {
        }

        protected override void ApplyConfig()
        {
            _drawAmount = KlondikeSettings.DrawAmount;
            _availableRestocks = KlondikeSettings.AvailableRestocks;
        }

        public override void OnRestock(bool undo = false)
        {
            if (_availableRestocks != -1)
            {
                if (undo) _availableRestocks++;
                else _availableRestocks--;
            }
        }
    }
}
