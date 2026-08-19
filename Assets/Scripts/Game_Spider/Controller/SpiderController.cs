using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Spider
{
    public class SpiderController : GameController
    {
        [SerializeField] private SpiderView _view;

        public override bool IsRestockAvailable()
        {
            return false;
        }

        protected override void CreateDeck()
        {
            throw new NotImplementedException();
        }

        protected override void CreateGame()
        {
            throw new NotImplementedException();
        }

        protected override void DoRefreshView(List<PileKind> pilesToRefresh, Action onDone, Card cardMoved = null, Vector3 originalCardPosition = default, bool immediate = false)
        {
            throw new NotImplementedException();
        }

        protected override void InitConfig()
        {
            throw new NotImplementedException();
        }

        protected override void InitGameView()
        {
            throw new NotImplementedException();
        }

        protected override bool IsAutoMovesEnabled()
        {
            throw new NotImplementedException();
        }

        protected override void ResetSettingsToDefault()
        {
            throw new NotImplementedException();
        }

        protected override void ResetView()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateWinsCount()
        {
            throw new NotImplementedException();
        }
    }
}