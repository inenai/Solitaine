using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    public class KlondikeController : GameController
    {
        [SerializeField] private KlondikeConfig _defaultConfig;

        public override bool IsRestockAvailable()
        {
            return ((KlondikeState)_game.State).AvailableRestocks != 0;
        }

        protected override void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        protected override bool IsAutoMovesEnabled()
        {
            return KlondikeSettings.AutoMovesEnabled;
        }

        protected override void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }

        protected override void LoadDeck()
        {
            _deck = KlondikeGame.CreateGameDeck();
        }

        protected override void LoadGame()
        {
            _game = new KlondikeGame(_deck);
        }

        protected override void UpdateWinsCount()
        {
            KlondikeSettings.WinCount++;
        }
    }
}
