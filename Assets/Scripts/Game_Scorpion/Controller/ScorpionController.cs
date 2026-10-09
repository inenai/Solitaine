using System;
using Common;
using Services;
using Storage;
using UnityEngine;

namespace Scorpion
{
    public class ScorpionController : GameController
    {
        [SerializeField] private ScorpionConfig _defaultConfig;

        protected override SolitaireKind _solitaireKind => SolitaireKind.SCORPION;

        protected override void StartNewGame(Action onDone, SavedGame progress)
        {
            LoadDeck();
            LoadDeckView(() =>
            {
                base.StartNewGame(() =>
                {
                    onDone.Invoke();
                }, progress);
            });
        }


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

        protected override void LoadGame(SavedGame progress)
        {
            _game = new ScorpionGame(_deck, progress);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry_Scorpion(
                 _game.State.TimeSpentSeconds,
                 _game.State.MovesCount,
                 _game.State.UsedUndos,
                 _game.State.UsedRedos,
                 ((ScorpionState)_game.State).Variant,
                 ((ScorpionState)_game.State).SuitsUsed);
        }
    }
}
