using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Klondike
{
    public class KlondikeState
    {
        public Stack<Card> StockPile;
        public Stack<Card> WastePile;
        public Stack<Card>[] Tableaus;
        public Foundation[] Foundations;

        public int DrawCount => _drawAmount;
        public int AvailableRestocks => _availableRestocks;
        public bool FoundationCardsFree => _foundationCardsFree;

        int _drawAmount = KlondikeSettings.DEFAULT_DRAW_AMOUNT;
        int _availableRestocks = KlondikeSettings.DEFAULT_RESTOCKS;
        bool _foundationCardsFree = KlondikeSettings.DEFAULT_FOUNDATION_CARDS_FREE;

        public void InitState()
        {
            ApplyConfig();
            InitFoundations();
            InitTableau();
            StockPile = new Stack<Card>();
            WastePile = new Stack<Card>();
        }

        private void InitFoundations()
        {
            Foundations = new Foundation[4];
            for (int i = 0; i < 4; i++)
            {
                Foundations[i] = new Foundation();
            }
        }

        private void InitTableau()
        {
            Tableaus = new Stack<Card>[7];
            for (int i = 0; i < 7; i++)
            {
                Tableaus[i] = new Stack<Card>();
            }
        }

        private void ApplyConfig()
        {
            _drawAmount = KlondikeSettings.DrawAmount;
            _availableRestocks = KlondikeSettings.AvailableRestocks;
            _foundationCardsFree = KlondikeSettings.FoundationCardsFree;
        }

        public PileData GetCardPileOwnerData(Card card)
        {
            PileKind kind = PileKind.WASTE;
            int index = -1;
            bool found = false;

            if (WastePile.Contains(card))
            {
                kind = PileKind.WASTE;
                found = true;
            }

            if (!found && StockPile.Contains(card))
            {
                kind = PileKind.STOCK;
                found = true;
            }

            if (!found)
            {
                for (int i = 0; i < Foundations.Length; i++)
                {
                    if (Foundations[i].Stack.Contains(card))
                    {
                        kind = PileKind.FOUNDATION;
                        index = i;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                for (int i = 0; i < Tableaus.Length; i++)
                {
                    if (Tableaus[i].Contains(card))
                    {
                        kind = PileKind.TABLEAU;
                        index = i;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                throw new Exception($"Card {card} not found in any pile!");
            }
            Debug.Log($"Card {card} found in {kind}[{index}]");
            return new PileData(kind, index);
        }

        public void OnRestocked()
        {
            if (_availableRestocks > 0)
                _availableRestocks--;
        }
    }
}
