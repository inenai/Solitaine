using Common;

namespace Klondike
{
    public class KlondikeState : GameState
    {
        public int DrawCount => _drawAmount;
        public override int AvailableRestocks => _availableRestocks;
        public override bool FoundationCardsFree => _foundationCardsFree;

        int _drawAmount = KlondikeSettings.DEFAULT_DRAW_AMOUNT;
        int _availableRestocks = KlondikeSettings.DEFAULT_RESTOCKS;
        bool _foundationCardsFree = KlondikeSettings.DEFAULT_FOUNDATION_CARDS_FREE;

        public KlondikeState(Game game) : base(game)
        {
        }

        protected override void ApplyConfig()
        {
            _drawAmount = KlondikeSettings.DrawAmount;
            _availableRestocks = KlondikeSettings.AvailableRestocks;
            _foundationCardsFree = KlondikeSettings.FoundationCardsFree;
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
