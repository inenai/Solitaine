
using Common;
using Services;

namespace Sawayama
{
    public class SawayamaController : GameController
    {
        private SawayamaGame SGame => (SawayamaGame)_game;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void LoadDeck()
        {
            _deck = SawayamaGame.CreateGameDeck();
        }
        protected override void LoadGame()
        {
            _game = new SawayamaGame(_deck);
        }


        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry(
               SolitaireKind.SAWAYAMA,
               TimeSpentSeconds,
               _game.State.MovesCount,
               _game.State.UsedUndos,
               _game.State.UsedRedos);
        }
    }
}
