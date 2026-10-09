using Common;
using Storage;

namespace Scorpion
{
    public class ScorpionState : GameState
    {
        public override int AvailableRestocks => 0;
        public override bool FoundationCardsFree => false;
        public int SuitsUsed => _suitsUsed;
        public override SolitaireKind SolitaireKind => SolitaireKind.SCORPION;
        private int _suitsUsed;

        public ScorpionVariant Variant => _variant;
        private ScorpionVariant _variant;

        public ScorpionState(Game game, SavedGame progress) : base(game, progress)
        {
        }

        protected override void ApplyConfig()
        {
            _variant = ScorpionSettings.Variant;
            _suitsUsed = ScorpionSettings.SuitsAmount;
        }
    }
}
