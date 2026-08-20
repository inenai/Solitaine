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
        [SerializeField] FreeCellView _freeCell;
        [SerializeField] FoundationView[] _foundations;
        [SerializeField] TableauView[] _tableaus;

        private int _refreshFoundationsCoroutinesRunning;
        private int _refreshTableausCoroutinesRunning;

        private bool _stockEmptied;

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
            _freeCell.Init(this, 0);
        }

        public override bool IsCardInATargetablePile(PileData cardOwnerData, out TargetCardPileView result)
        {
            result = null;
            switch (cardOwnerData.Kind)
            {
                case PileKind.WASTE:
                    return false;
                case PileKind.STOCK:
                    return false;
                case PileKind.FREECELL:
                    result = _freeCell;
                    return true;
                case PileKind.FOUNDATION:
                    result = _foundations[cardOwnerData.Index];
                    return true;
                case PileKind.TABLEAU:
                    result = _tableaus[cardOwnerData.Index];
                    return true;
            }
            return false;
        }

        public override void RefreshStock(CardPile stockCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _stock.Refresh(stockCards, cardMoved, movedCardOriginalPosition, immediate, onDone);
            OnStockEmpty(stockCards.Count == 0);
        }

        public override void RefreshWaste(CardPile wasteCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
        {
            _waste.Refresh(wasteCards, cardMoved, movedCardOriginalPosition, immediate, onDone);
        }

        public override void RefreshFoundations(List<CardPile> foundationsCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
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

        public override void RefreshTableaus(List<CardPile> tableausCards, Card cardMoved, Vector3 movedCardOriginalPosition, bool immediate, Action onDone)
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

        public override void RefreshFreeCells(List<CardPile> list, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action refreshDone)
        {
            _freeCell.Refresh(list[0], cardMoved, originalCardPosition, immediate, refreshDone);
        }

        private void OnStockEmpty(bool empty)
        {
            if (empty == _stockEmptied) return;
            if (!empty)
            {
                _stockEmptied = false;
                Reset();
                return;
            }

            _freeCell.gameObject.SetActive(true);
            _stock.gameObject.SetActive(false);
            _stockEmptied = true;
        }

        public override void Reset()
        {
            _freeCell.gameObject.SetActive(false);
            _stock.gameObject.SetActive(true);
            _stockEmptied = false;
        }
    }
}