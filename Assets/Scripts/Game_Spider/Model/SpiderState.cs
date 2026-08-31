using Common;

namespace Spider
{
    public class SpiderState : GameState
    {
        public SpiderState(Game game, int suits) : base(game)
        {
            SuitsUsed = suits;
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => false;

        #region Statistics
        public int SuitsUsed { get; private set; }
        #endregion
    }
}