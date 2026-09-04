using Common;

namespace Spider
{
    public class SpiderState : GameState
    {
        private int _suitsUsed;
        public int SuitsUsed => _suitsUsed;
        public SpiderState(Game game) : base(game)
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