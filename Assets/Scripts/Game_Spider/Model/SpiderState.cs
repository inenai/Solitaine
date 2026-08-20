using Common;

namespace Spider
{
    public class SpiderState : GameState
    {
        public SpiderState(Game game) : base(game)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => false;

    }
}