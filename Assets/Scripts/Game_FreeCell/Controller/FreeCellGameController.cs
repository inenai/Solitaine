using Common;
using Services;

namespace FreeCell
{
    public class FreeCellGameController : GameController
    {
        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void LoadDeck()
        {
            _deck = FreeCellGameGame.CreateGameDeck();
        }

        protected override void LoadGame()
        {
            _game = new FreeCellGameGame(_deck);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry(
                SolitaireKind.FREECELL,
                TimeSpentSeconds,
                _game.State.MovesCount,
                _game.State.UsedUndos,
                _game.State.UsedRedos);
        }
    }
}