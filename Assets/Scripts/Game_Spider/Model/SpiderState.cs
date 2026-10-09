using Common;
using Storage;

namespace Spider
{
    public class SpiderState : GameState
    {
        public override SolitaireKind SolitaireKind => SolitaireKind.SPIDER;
        private int _suitsUsed;
        public int SuitsUsed => _suitsUsed;
        public SpiderState(Game game, SavedGame progress) : base(game, progress)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => false;

        protected override void ApplyConfig()
        {
            _suitsUsed = SpiderSettings.SuitsAmount;
        }
    }
}