using UnityEngine;
using System;

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

        protected override bool IsAutoMovesEnabled()
        {
            return true;
        }

        protected override void UpdateWinsCount()
        {
            SpiderSettings.WinCount++;
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