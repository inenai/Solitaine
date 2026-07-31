using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;

namespace Klondike
{
    public class KlondikeController : GameController
    {
        [SerializeField] private KlondikeConfig _defaultConfig;
        [SerializeField] private KlondikeView _view;

        private KlondikeGame KGame => (KlondikeGame)_game;

        public override void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        public override bool IsRestockAvailable()
        {
            return ((KlondikeState)_game.State).AvailableRestocks != 0;
        }

        public override bool IsAutoMovesEnabled()
        {
            return KlondikeSettings.AutoMovesEnabled;
        }

        protected override void CreateGame()
        {
            _game = new KlondikeGame(_deck);
        }

        protected override void InitConfig()
        {
            if (!KlondikeSettings.SavedSettingsAvailable)
            {
                _defaultConfig.ApplyDefaultSettings();
            }
        }

        protected override void InitGameView()
        {
            _gameView = _view;
            _view.Init(this);
        }

        protected override void CreateDeck()
        {
            _deck = KlondikeGame.CreateGameDeck();
        }

        protected override void UpdateWinsCount()
        {
            KlondikeSettings.WinCount++;
        }

        protected override void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false)
        {
            Debug.Log("RefreshViewTask.");
            _viewsRefreshing = 0;

            void RefreshDone() { _viewsRefreshing--; }

            foreach (PileKind kind in pilesToRefresh)
            {
                switch (kind)
                {
                    case PileKind.WASTE:
                        _viewsRefreshing++;
                        _view.RefreshWaste(
                            CommonUtils.CloneStack(KGame.KState.WastePile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.STOCK:
                        _viewsRefreshing++;
                        _view.RefreshStock(
                            CommonUtils.CloneStack(KGame.KState.StockPile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FOUNDATION:
                        _viewsRefreshing++;
                        _view.RefreshFoundations(
                            CardUtils.GetClonedStacks(KGame.KState.Foundations),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(KGame.KState.Tableaus),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;
                }
            }
            Debug.Log("Await...");
            StartCoroutine(WaitForViewsToBeRefreshed(onDone));
        }

        private IEnumerator WaitForViewsToBeRefreshed(Action onDone)
        {
            while (_viewsRefreshing > 0) yield return null;
            Debug.Log("Done.");
            onDone?.Invoke();
        }

        protected override void ResetView()
        {

        }
    }
}
