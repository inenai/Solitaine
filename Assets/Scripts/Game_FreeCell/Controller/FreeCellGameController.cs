
using System.Collections.Generic;
using Common;

namespace FreeCell
{
    public class FreeCellGameController : GameController
    {
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
            _deck = FreeCellGameGame.CreateGameDeck();
        }

        protected override void LoadGame()
        {
            _game = new FreeCellGameGame(_deck);
        }

        protected override void UpdateWinsCount()
        {
            FreeCellGameSettings.WinCount++;
        }
    }
}