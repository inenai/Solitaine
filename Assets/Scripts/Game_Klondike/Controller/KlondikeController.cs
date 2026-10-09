using Common;
using Services;
using Storage;
using UnityEngine;

namespace Klondike
{
    public class KlondikeController : GameController
    {
        [SerializeField] private KlondikeConfig _defaultConfig;

        protected override SolitaireKind _solitaireKind => SolitaireKind.KLONDIKE;

        public override bool IsRestockAvailable()
        {
            return ((KlondikeState)_game.State).AvailableRestocks != 0;
        }

        protected override void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
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

        protected override void LoadGame(SavedGame progress)
        {
            _game = new KlondikeGame(_deck, progress);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry_Klondike(
                 _game.State.TimeSpentSeconds,
                 _game.State.MovesCount,
                 _game.State.UsedUndos,
                 _game.State.UsedRedos,
                 ((KlondikeState)_game.State).DrawAmount,
                 ((KlondikeState)_game.State).AvailableRestocks);
        }
    }
}
