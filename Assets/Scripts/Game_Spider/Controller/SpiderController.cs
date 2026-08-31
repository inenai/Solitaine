using UnityEngine;
using System;
using Services;

namespace Spider
{
    public class SpiderController : GameController
    {
        [SerializeField] private SpiderConfig _defaultConfig;
        protected override void StartNewGame(Action onDone)
        {
            LoadDeck();
            LoadDeckView(()=>
            {
                base.StartNewGame(() =>
                {
                    onDone.Invoke();
                });
            });
        }

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

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry_Spider(
                TimeSpentSeconds,
                _game.State.MovesCount,
                _game.State.UsedUndos,
                _game.State.UsedRedos,
                ((SpiderState)_game.State).SuitsUsed);
        }

        protected override void InitConfig()
        {
            if (!SpiderSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }
    }
}