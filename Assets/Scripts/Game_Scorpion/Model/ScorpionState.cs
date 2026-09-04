using Common;

namespace Scorpion
{
    public class ScorpionState : GameState
    {
        public override int AvailableRestocks => 0;
        public override bool FoundationCardsFree => false;
        public int SuitsUsed => _suitsUsed;
        private int _suitsUsed;

        public ScorpionVariant Variant => _variant;
        private ScorpionVariant _variant;

        public ScorpionState(Game game) : base(game)
        {
        }

        protected override void ApplyConfig()
        {
            _variant = ScorpionSettings.Variant;
            _suitsUsed = ScorpionSettings.SuitsAmount;
        }
    }
}
