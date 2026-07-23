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
        private FreeCellGameGame _game;
        public override bool IsAutoMovesEnabled()
        {
            return true;
        }

        public override bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
        }

        public override bool IsCardInTargetPile(Card card, out TargetCardPileView result)
        {
            return _view.IsCardInTargetPile(_game.FCState.GetCardPileOwnerData(card), out result);
        }

        public override bool IsRestockAvailable()
        {
            return false;
        }

        public override void ResetSettingsToDefault() { }

        protected override List<PileKind> Action_DoubleClickedCard(Card card)
        {
            return _game.Action_TryMoveCardAutomatic(card);
        }

        protected override List<PileKind> Action_DragCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            return _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex);
        }

        protected override Func<List<PileKind>> Action_PileClicked(PileKind kind)
        {
            return null;
        }

        protected override List<PileKind> Auto_MoveCardAutomatically(Card card)
        {
            return _game.Auto_TryMoveCardToFoundationAutomatic(card);
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
                            CardUtils.GetClonedStacks(_game.FCState.Foundations),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(_game.FCState.Tableaus),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FREECELL:
                        _viewsRefreshing++;
                        _view.RefreshFreeCells(
                            CardUtils.GetClonedStacks(_game.FCState.FreeCells),
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

        protected override Vector3 GetCardViewPosition(Card card)
        {
            return _view.GetCardViewPosition(card);
        }

        protected override Card GetSolvableCard()
        {
            return _game.GetSolvableCard();
        }

        protected override void InitConfig() { }

        protected override void InitView()
        {
            _view.Init(this);
        }

        protected override void LoadDeckView(Action onDone)
        {
            Debug.Log("LoadDeckView.");
            _view.Deck.Load(_deck, _view.FirstTableauTransform, onDone);
        }

        protected override void ResetView() { }

        protected override void UpdateWinsCount()
        {
            FreeCellGameSettings.WinCount++;
        }
    }
}