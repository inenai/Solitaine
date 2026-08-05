using System.Linq;
using Common;

namespace FreeCell
{
    public class FreeCellGameState : GameState
    {
        public FreeCellGameState(int foundations, int tableaus, int freeCells, bool stock, bool waste) : base(foundations, tableaus, freeCells, stock, waste)
        {
        }

        public int FreeMovingSpaces => FreeCells.Count(card => card.Count == 0) + Tableaus.Count(card => card.Count == 0);

        protected override void ApplyConfig()
        {

        }
    }
}