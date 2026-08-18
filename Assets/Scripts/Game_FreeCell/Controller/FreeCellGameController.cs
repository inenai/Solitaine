using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;
namespace FreeCell
{
    public class FreeCellGameController : GameController
    {
        [SerializeField] private FreeCellGameView _view;

        private FreeCellGameGame FCGame => (FreeCellGameGame)_game;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override bool IsAutoMovesEnabled()
        {
            return true;
        }

        protected override void ResetSettingsToDefault()
        {

        }

        protected override void CreateDeck()
        {
            _deck = FreeCellGameGame.CreateGameDeck();
        }

        protected override void CreateGame()
        {
            _game = new FreeCellGameGame(_deck);
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
                    case PileKind.FOUNDATION:
                        _viewsRefreshing++;
                        _view.RefreshFoundations(
                            CardUtils.GetClonedStacks(FCGame.FCState.Foundations),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(FCGame.FCState.Tableaus),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FREECELL:
                        _viewsRefreshing++;
                        _view.RefreshFreeCells(
                            CardUtils.GetClonedStacks(FCGame.FCState.FreeCells),
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

        protected override void InitConfig()
        {

        }

        protected override void InitGameView()
        {
            _gameView = _view;
            _view.Init(this);
        }

        protected override void ResetView()
        {

        }

        protected override void UpdateWinsCount()
        {
            FreeCellGameSettings.WinCount++;
        }
    }
}