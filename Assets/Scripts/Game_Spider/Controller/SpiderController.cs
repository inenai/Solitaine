using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Spider
{
    public class SpiderController : GameController
    {
        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void LoadDeck()
        {
            _deck = SpiderGame.CreateGameDeck();
        }

        protected override void LoadGame()
        {
            _game = new SpiderGame(_deck);
        }

        protected override bool IsAutoMovesEnabled()
        {
            return true;
        }

        protected override void UpdateWinsCount()
        {
            SpiderSettings.WinCount++;
        }
    }
}