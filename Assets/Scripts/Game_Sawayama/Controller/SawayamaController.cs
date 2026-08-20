
namespace Sawayama
{
    public class SawayamaController : GameController
    {
        private SawayamaGame SGame => (SawayamaGame)_game;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override bool IsAutoMovesEnabled()
        {
            return true;
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
            SawayamaSettings.WinCount++;
        }
    }
}
