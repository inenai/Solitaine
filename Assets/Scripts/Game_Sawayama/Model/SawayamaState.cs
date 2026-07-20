using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Sawayama
{
    public class SawayamaState : IGameState
    {
        public Stack<Card> Deck;
        public Stack<Card> StockPile;
        public Stack<Card> WastePile;
        public Stack<Card>[] Tableaus;
        public Foundation[] Foundations;

        public SawayamaState()
        {
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
            //Debug.Log($"Card {card} found in {kind}[{index}]");
            return new PileData(kind, index);
        }

        public void OnWin()
        {
            foreach (Foundation f in Foundations)
            {
                foreach (Card card in f.Stack)
                {
                    card.FreeCard(false);
                }
            }
        }

        public void LogState()
        {
            Debug.Log($"Stock ({StockPile.Count}): {string.Join(" ", StockPile)}");
            if (StockPile.Count > 0) Debug.Log($"Top card in stock is: {StockPile.Peek()}");
            Debug.Log($"Waste ({WastePile.Count}): {string.Join(" ", WastePile)}");
            if (WastePile.Count > 0) Debug.Log($"Top card in waste is: {WastePile.Peek()}");

            Debug.Log("Foundations:");
            for (int i = 0; i < Foundations.Length; i++)
            {
                Debug.Log($"  F{i}: {string.Join(" ", Foundations[i].Stack)}");
                if (Foundations[i].Stack.Count > 0) Debug.Log($"Top card in F{i} is: {Foundations[i].Stack.Peek()}");
            }

            Debug.Log("Tableaus:");
            for (int i = 0; i < Tableaus.Length; i++)
            {
                Debug.Log($"  T{i}: {string.Join(" ", Tableaus[i])}");
                if (Tableaus[i].Count > 0) Debug.Log($"Top card in T{i} is: {Tableaus[i].Peek()}");
            }
        }
    }
}