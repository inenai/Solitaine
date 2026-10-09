using Common;
using Services;
using Storage;

namespace Sawayama
{
    public class SawayamaController : GameController
    {
        protected override SolitaireKind _solitaireKind => SolitaireKind.SAWAYAMA;
        private SawayamaGame SGame => (SawayamaGame)_game;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void LoadDeck()
        {
            _deck = SawayamaGame.CreateGameDeck();
        }
        protected override void LoadGame(SavedGame progress)
        {
            _game = new SawayamaGame(_deck, progress);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry(
               SolitaireKind.SAWAYAMA,
               _game.State.TimeSpentSeconds,
               _game.State.MovesCount,
               _game.State.UsedUndos,
               _game.State.UsedRedos);
        }
    }
}
