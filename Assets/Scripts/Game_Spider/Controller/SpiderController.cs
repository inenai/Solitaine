using UnityEngine;
using System;
using Services;
using Common;
using Storage;

namespace Spider
{
    public class SpiderController : GameController
    {
        [SerializeField] private SpiderConfig _defaultConfig;

        protected override SolitaireKind _solitaireKind => SolitaireKind.SPIDER;

        protected override void StartNewGame(Action onDone, SavedGame progress)
        {
            LoadDeck();
            LoadDeckView(()=>
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

        protected override void LoadDeck()
        {
            _deck = SpiderGame.CreateGameDeck();
        }

        protected override void LoadGame(SavedGame progress)
        {
            _game = new SpiderGame(_deck, progress);
        }

        protected override void UpdateWinsCount()
        {
            God.Database.AddGameEntry_Spider(
                _game.State.TimeSpentSeconds,
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