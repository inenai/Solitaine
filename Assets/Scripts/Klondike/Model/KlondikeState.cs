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
        public Stack<Card>[] Tableau;
        public Foundation[] Foundations;

        int _drawCount = 1;
        bool _allowRestock = true;

        public void InitState()
        {
            InitFoundations();
            InitTableau();
            StockPile = new Stack<Card>();
            WastePile = new Stack<Card>();

            ApplyConfig();
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
            Tableau = new Stack<Card>[7];
            for (int i = 0; i < 7; i++)
            {
                Tableau[i] = new Stack<Card>();
            }
        }

        private void ApplyConfig()
        {
            _drawCount = KlondikeConfig.DrawAmount;
            _allowRestock = KlondikeConfig.AllowRedraw;
        }

        public int DrawCount => _drawCount;
        public bool AllowRestock => _allowRestock;

        public CardPileData GetCardPileOwnerData(Card card)
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
                for (int i = 0; i < Tableau.Length; i++)
                {
                    if (Tableau[i].Contains(card))
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
            return new CardPileData(kind, index);
        }
    }
}
