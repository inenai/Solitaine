using System.Linq;
using Common;

namespace FreeCell
{
    public class FreeCellGameState : GameState
    {
        public FreeCellGameState(Game game) : base(game)
        {
        }

        public int FreeMovingSpaces => FreeCells.Count(card => card.Count == 0) + Tableaus.Count(card => card.Count == 0);

        public override int AvailableRestocks => 0;

        protected override void ApplyConfig()
        {

        }
    }
}