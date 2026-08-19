using System.Collections.Generic;
using System.Linq;
using Common;

namespace FreeCell
{
    public class FreeCellGameState : GameState
    {
        public FreeCellGameState(Game game) : base(game)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => true;

        protected override void ApplyConfig()
        {

        }

        public int GetFreeMovingSpaces()
        {
            int total = 0;
            foreach (Stack<Card> freeCell in FreeCells)
            {
                if (freeCell.Count == 0) total++;
            }

            foreach (Stack<Card> tableau in Tableaus)
            {
                if (tableau.Count == 0) total++;
            }
            return total;
        }
    }
}