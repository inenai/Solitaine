using Common;

namespace Sawayama
{
    public class SawayamaState : GameState
    {
        public SawayamaState(Game game) : base(game)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => false;

        protected override void ApplyConfig()
        {
        }
    }
}