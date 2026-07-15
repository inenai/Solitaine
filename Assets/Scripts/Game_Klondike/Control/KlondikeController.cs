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
        private int _viewsRefreshing = 0;
        private KlondikeGame _game;

        public override bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            return _game.CanAddCardToPile(card, targetPile, targetPileIndex);
        }

        public override bool IsCardInTargetPile(Card card, out TargetCardPileView result)
        {
            return _view.IsCardInTargetPile(_game.State.GetCardPileOwnerData(card), out result);
        }

        public override void ResetSettingsToDefault()
        {
            KlondikeSettings.Reset(_defaultConfig);
        }

        public override bool IsRestockAvailable()
        {
            return _game.State.AvailableRestocks != 0;
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

        protected override void InitView()
        {
            _view.Init(this);
        }

        protected override void CreateDeck()
        {
            _deck = KlondikeGame.CreateGameDeck();
        }

        protected override void LoadDeckView(Action onDone)
        {
            Debug.Log("LoadDeckView.");
            _view.Deck.Load(_deck, onDone);
        }

        protected override void UpdateWinsCount()
        {
            KlondikeSettings.WinCount++;
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

        protected override List<PileKind> Action_DoubleClickedCard(Card card)
        {
            return _game.Action_TryMoveCardAutomatic(card);
        }

        protected override List<PileKind> Action_DragCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            return _game.Action_TryMoveCardToPile(card, targetPileKind, targetPileIndex);
        }

        protected override void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool isInitRefresh = false)
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
                            CommonUtils.CloneStack(_game.State.WastePile),
                            cardMoved,
                            originalCardPosition,
                            isInitRefresh,
                            RefreshDone);
                        break;

                    case PileKind.STOCK:
                        _viewsRefreshing++;
                        _view.RefreshStock(
                            CommonUtils.CloneStack(_game.State.StockPile),
                            cardMoved,
                            originalCardPosition,
                            isInitRefresh,
                            RefreshDone);
                        break;

                    case PileKind.FOUNDATION:
                        _viewsRefreshing++;
                        _view.RefreshFoundations(
                            CardUtils.GetClonedStacks(_game.State.Foundations),
                            cardMoved,
                            originalCardPosition,
                            isInitRefresh,
                            RefreshDone);
                        break;

                    case PileKind.TABLEAU:
                        _viewsRefreshing++;
                        _view.RefreshTableaus(
                            CardUtils.GetClonedStacks(_game.State.Tableaus),
                            cardMoved,
                            originalCardPosition,
                            isInitRefresh,
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
    }
}