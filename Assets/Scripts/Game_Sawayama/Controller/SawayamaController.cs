using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;

namespace Sawayama
{
    public class SawayamaController : GameController
    {
        [SerializeField] private SawayamaView _view;
        private SawayamaGame SGame => (SawayamaGame)_game;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override bool IsAutoMovesEnabled()
        {
            return true;
        }

        protected override void ResetSettingsToDefault() {}

        protected override void CreateDeck()
        {
            _deck = SawayamaGame.CreateGameDeck();
        }
        protected override void CreateGame()
        {
            _game = new SawayamaGame(_deck);
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
                            CommonUtils.CloneStack(SGame.SState.WastePile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.STOCK:
                        _viewsRefreshing++;
                        _view.RefreshStock(
                            CommonUtils.CloneStack(SGame.SState.StockPile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FOUNDATION:
                        _viewsRefreshing++;
                        _view.RefreshFoundations(
                            CardUtils.GetClonedStacks(SGame.SState.Foundations),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(SGame.SState.Tableaus),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FREECELL:
                        _viewsRefreshing++;
                        _view.RefreshFreeCells(
                            CommonUtils.CloneStack(SGame.SState.FreeCells[0]),
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

        protected override void InitConfig() { }
        protected override void InitGameView()
        {
            _gameView = _view;
            _view.Init(this);
        }

        protected override void UpdateWinsCount()
        {
            SawayamaSettings.WinCount++;
        }

        protected override void ResetView()
        {
            _view.Reset();
        }
    }
}
