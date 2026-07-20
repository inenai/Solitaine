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
        private int _viewsRefreshing = 0;
        private SawayamaGame _game;

        public override bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
        }

        public override bool IsCardInTargetPile(Card card, out TargetCardPileView result)
        {
            return _view.IsCardInTargetPile(_game.State.GetCardPileOwnerData(card), out result);
        }

        public override bool IsRestockAvailable()
        {
            return false;
        }

        public override bool IsAutoMovesEnabled()
        {
            return true;
        }

        public override void ResetSettingsToDefault() {}

        protected override List<PileKind> Action_DoubleClickedCard(Card card)
        {
            return new List<PileKind>();
        }

        protected override List<PileKind> Action_DragCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            return _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex);
        }

        protected override Func<List<PileKind>> Action_PileClicked(PileKind pileKind)
        {
            Func<List<PileKind>> action = pileKind switch
            {
                PileKind.STOCK => _game.Action_TryDrawCardsFromStock,
                _ => null
            };
            return action;
        }

        protected override List<PileKind> Auto_MoveCardAutomatically(Card card)
        {
            return _game.Auto_MoveCardAutomatically(card);
        }

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
                            CommonUtils.CloneStack(_game.SState.WastePile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.STOCK:
                        _viewsRefreshing++;
                        _view.RefreshStock(
                            CommonUtils.CloneStack(_game.SState.StockPile),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.FOUNDATION:
                        _viewsRefreshing++;
                        _view.RefreshFoundations(
                            CardUtils.GetClonedStacks(_game.SState.Foundations),
                            cardMoved,
                            originalCardPosition,
                            immediate,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(_game.SState.Tableaus),
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
        protected override void InitView()
        {
            _view.Init(this);
        }

        protected override void LoadDeckView(Action onDone)
        {
            Debug.Log("LoadDeckView.");
            _view.Deck.Load(_deck, _view.StockTransform, onDone);
        }

        protected override void UpdateWinsCount()
        {
            SawayamaSettings.WinCount++;
        }

        protected override void ResetView()
        {
            _view.Reset();
        }

        protected override Card GetSolvableCard()
        {
           return _game.GetSolvableCard();
        }

        protected override Vector3 GetCardViewPosition(Card card)
        {
            return _view.GetCardViewPosition(card);
        }
    }
}
