using Services;
using UnityEngine;

namespace Scorpion
{
    public class ScorpionController : GameController
    {
        [SerializeField] private ScorpionConfig _defaultConfig;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void ResetSettingsToDefault()
        {
            ScorpionSettings.Reset(_defaultConfig);
        }

        protected override void InitConfig()
        {
            if (!ScorpionSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }

        protected override void LoadDeck()
        {
            _deck = ScorpionGame.CreateGameDeck();
        }

        protected override void LoadGame()
        {
            _game = new ScorpionGame(_deck);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry_Scorpion(
                 TimeSpentSeconds,
                 _game.State.MovesCount,
                 _game.State.UsedUndos,
                 _game.State.UsedRedos,
                 ((ScorpionState)_game.State).Variant);
        }
    }
}
