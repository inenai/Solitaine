using Common;
using Storage;

namespace FreeCell
{
    public class FreeCellGameState : GameState
    {
        public FreeCellGameState(Game game, SavedGame progress) : base(game, progress)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => true;

        public override SolitaireKind SolitaireKind => SolitaireKind.FREECELL;

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