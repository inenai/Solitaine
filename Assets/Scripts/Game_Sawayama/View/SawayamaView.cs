using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Sawayama
{
    public class SawayamaView : GameView
    {
        //View
        [SerializeField] StockView _stock;
        [SerializeField] WasteView _waste;
        [SerializeField] FoundationView[] _foundations;
        [SerializeField] TableauView[] _tableaus;

        private int _refreshFoundationsCoroutinesRunning;
        private int _refreshTableausCoroutinesRunning;
        private bool _stockEmptied;

        void Awake()
        {
            EventManager.OnStockEmpty += OnStockEmpty;
        }

        protected override void OnInit()
        {
            _stock.Init(this);
            _waste.Init(this);
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationView foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i);
            }
        }

        void OnDestroy()
        {
            EventManager.OnStockEmpty -= OnStockEmpty;
        }

        public override bool IsCardInTargetPile(PileData cardOwnerData, out TargetCardPileView result)
        {
            result = null;
            switch (cardOwnerData.Kind)
            {
                case PileKind.WASTE:
                    return false;
                case PileKind.STOCK:
                    return false;
                case PileKind.FOUNDATION:
                    result = _foundations[cardOwnerData.Index];
                    return true;
                case PileKind.TABLEAU:
                    result = _tableaus[cardOwnerData.Index];
                    return true;
            }
            return false;
        }

        public void RefreshStock(Stack<Card> stockCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _stock.Refresh(stockCards, cardMoved, movedCardOriginalPosition, immediate, onDone);
        }

        public void RefreshWaste(Stack<Card> wasteCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _waste.Refresh(wasteCards, cardMoved, movedCardOriginalPosition, immediate, onDone);
        }

        public void RefreshFoundations(List<Stack<Card>> foundationsCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _refreshFoundationsCoroutinesRunning = 0;
            for (int i = 0; i < _foundations.Length; i++)
            {
                _refreshFoundationsCoroutinesRunning++;
                _foundations[i].Refresh(foundationsCards[i], cardMoved, movedCardOriginalPosition, immediate, () =>
                {
                    _refreshFoundationsCoroutinesRunning--;
                });
            }
            StartCoroutine(WaitForFoundationsRefreshedCR(onDone));
        }

        private IEnumerator WaitForFoundationsRefreshedCR(Action onDone)
        {
            while (_refreshFoundationsCoroutinesRunning > 0) yield return null;
            onDone?.Invoke();
        }

        public void RefreshTableaus(List<Stack<Card>> tableausCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _refreshTableausCoroutinesRunning = 0;
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _refreshTableausCoroutinesRunning++;
                _tableaus[i].Refresh(tableausCards[i], cardMoved, movedCardOriginalPosition, immediate, () =>
                {
                    _refreshTableausCoroutinesRunning--;
                });
            }
            StartCoroutine(WaitForTableausRefreshedCR(onDone));
        }

        private IEnumerator WaitForTableausRefreshedCR(Action onDone)
        {
            while (_refreshTableausCoroutinesRunning > 0) yield return null;
            onDone?.Invoke();
        }

        private void OnStockEmpty()
        {
            if (_stockEmptied) return;
            _stock.transform.localPosition = new Vector3(_stock.transform.localPosition.x, _stock.transform.localPosition.y, 0f);
            _stockEmptied = true;
        }

        public void Reset()
        {
            _stock.transform.localPosition = new Vector3(_stock.transform.localPosition.x, _stock.transform.localPosition.y, -6f);
            _stockEmptied = false;
        }
    }
}