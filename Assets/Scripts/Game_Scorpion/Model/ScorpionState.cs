using Common;

namespace Scorpion
{
    public class ScorpionState : GameState
    {
        public override int AvailableRestocks => 0;
        public override bool FoundationCardsFree => false;

        public ScorpionVariant Variant => _variant;
        private ScorpionVariant _variant;

        public ScorpionState(Game game) : base(game)
        {
        }

        protected override void ApplyConfig()
        {
            _variant = ScorpionSettings.Variant;
        }
    }
}
