using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Sawayama
{
    public class SawayamaController : GameController
    {
        [SerializeField] private SawayamaView _view;
        private int _viewsRefreshing = 0;
        private SawayamaGame _game;

        public override bool IsCardAllowedInPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            throw new NotImplementedException();
        }

        public override bool IsCardInTargetPile(Card card, out TargetCardPileView result)
        {
            throw new NotImplementedException();
        }

        public override bool IsRestockAvailable()
        {
            return false;
        }

        public override void ResetSettingsToDefault() {}

        protected override List<PileKind> Action_DoubleClickedCard(Card card)
        {
            throw new NotImplementedException();
        }

        protected override List<PileKind> Action_DragCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            throw new NotImplementedException();
        }

        protected override Func<List<PileKind>> Action_PileClicked(PileKind kind)
        {
            throw new NotImplementedException();
        }

        protected override void CreateDeck()
        {
            throw new NotImplementedException();
        }

        protected override void CreateGame()
        {
            throw new NotImplementedException();
        }

        protected override void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool isInitRefresh = false)
        {
            throw new NotImplementedException();
        }

        protected override void InitConfig() { }

        protected override void InitView()
        {
            throw new NotImplementedException();
        }

        protected override void LoadDeckView(Action onDone)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateWinsCount()
        {
            throw new NotImplementedException();
        }
    }
}
