using Common;
using Services;
using Storage;

namespace FreeCell
{
    public class FreeCellGameController : GameController
    {
        protected override SolitaireKind _solitaireKind => SolitaireKind.FREECELL;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void LoadDeck()
        {
            _deck = FreeCellGameGame.CreateGameDeck();
        }

        protected override void LoadGame(SavedGame progress)
        {
            _game = new FreeCellGameGame(_deck, progress);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry(
                SolitaireKind.FREECELL,
                _game.State.TimeSpentSeconds,
                _game.State.MovesCount,
                _game.State.UsedUndos,
                _game.State.UsedRedos);
        }
    }
}