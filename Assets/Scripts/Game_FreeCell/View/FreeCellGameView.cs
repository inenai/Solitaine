using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace FreeCell
{
    public class FreeCellGameView : GameView
    {
        //View
        [SerializeField] FreeCellView[] _freeCells;
        [SerializeField] FoundationView[] _foundations;
        [SerializeField] TableauView[] _tableaus;

        public Transform FirstTableauTransform => _tableaus[0].transform;

        private int _refreshFoundationsCoroutinesRunning;
        private int _refreshTableausCoroutinesRunning;
        private int _refreshFreeCellsCoroutinesRunning;

        public override bool IsCardInTargetPile(PileData cardOwnerData, out TargetCardPileView result)
        {
            result = null;
            switch (cardOwnerData.Kind)
            {
                case PileKind.FOUNDATION:
                    result = _foundations[cardOwnerData.Index];
                    return true;
                case PileKind.TABLEAU:
                    result = _tableaus[cardOwnerData.Index];
                    return true;
                case PileKind.FREECELL:
                    result = _freeCells[cardOwnerData.Index];
                    return true;
            }
            return false;
        }

        protected override void OnInit()
        {
            for (int i = 0; i < _foundations.Length; i++)
            {
                FoundationView foundation = _foundations[i];
                foundation.Init(this, i);
            }
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _tableaus[i].Init(this, i);
            }
            for (int i = 0; i < _freeCells.Length; i++)
            {
                _freeCells[i].Init(this, i);
            }
        }

        public void RefreshFoundations(List<Stack<Card>> list, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action refreshDone)
        {
            _refreshFoundationsCoroutinesRunning = 0;
            for (int i = 0; i < _foundations.Length; i++)
            {
                _refreshFoundationsCoroutinesRunning++;
                _foundations[i].Refresh(list[i], cardMoved, originalCardPosition, immediate, () =>
                {
                    _refreshFoundationsCoroutinesRunning--;
                });
            }
            StartCoroutine(WaitForFoundationsRefreshedCR(refreshDone));
        }

        private IEnumerator WaitForFoundationsRefreshedCR(Action onDone)
        {
            while (_refreshFoundationsCoroutinesRunning > 0) yield return null;
            onDone?.Invoke();
        }

        public void RefreshFreeCells(List<Stack<Card>> list, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action refreshDone)
        {
            _refreshFreeCellsCoroutinesRunning = 0;
            for (int i = 0; i < _freeCells.Length; i++)
            {
                _refreshFreeCellsCoroutinesRunning++;
                _freeCells[i].Refresh(list[i], cardMoved, originalCardPosition, immediate, () =>
                {
                    _refreshFreeCellsCoroutinesRunning--;
                });
            }
            StartCoroutine(WaitFoFreeCellsRefreshedCR(refreshDone));
        }

        private IEnumerator WaitFoFreeCellsRefreshedCR(Action onDone)
        {
            while (_refreshFreeCellsCoroutinesRunning > 0) yield return null;
            onDone?.Invoke();
        }
        public void RefreshTableaus(List<Stack<Card>> list, Card cardMoved, Vector3 originalCardPosition, bool immediate, Action refreshDone)
        {
            _refreshTableausCoroutinesRunning = 0;
            for (int i = 0; i < _tableaus.Length; i++)
            {
                _refreshTableausCoroutinesRunning++;
                _tableaus[i].Refresh(list[i], cardMoved, originalCardPosition, immediate, () =>
                {
                    _refreshTableausCoroutinesRunning--;
                });
            }
            StartCoroutine(WaitForTableausRefreshedCR(refreshDone));
        }

        private IEnumerator WaitForTableausRefreshedCR(Action onDone)
        {
            while (_refreshTableausCoroutinesRunning > 0) yield return null;
            onDone?.Invoke();
        }
    }
}