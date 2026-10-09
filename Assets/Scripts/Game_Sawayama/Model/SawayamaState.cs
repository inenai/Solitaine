using Common;
using Storage;

namespace Sawayama
{
    public class SawayamaState : GameState
    {
        public SawayamaState(Game game, SavedGame progress) : base(game, progress)
        {
        }

        public override int AvailableRestocks => 0;

        public override bool FoundationCardsFree => false;

        public override SolitaireKind SolitaireKind => SolitaireKind.SAWAYAMA;

    }
}