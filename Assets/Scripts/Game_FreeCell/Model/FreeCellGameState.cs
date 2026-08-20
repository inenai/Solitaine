using System.Collections.Generic;
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

        public int GetFreeMovingSpaces()
        {
            int total = 0;
            foreach (CardPile freeCell in FreeCells)
            {
                if (freeCell.Count == 0) total++;
            }

            foreach (CardPile tableau in Tableaus)
            {
                if (tableau.Count == 0) total++;
            }
            return total;
        }
    }
}