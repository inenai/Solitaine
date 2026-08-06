using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public abstract class GameState
    {
        public Stack<Card> Deck;
        public Stack<Card> StockPile;
        public Stack<Card> WastePile;
        public Stack<Card>[] FreeCells;
        public Stack<Card>[] Tableaus;
        public Foundation[] Foundations;

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
        protected abstract void ApplyConfig();
        public virtual void OnRestock(bool undo = false){}

        public GameState(int foundations, int tableaus, int freeCells, bool stock, bool waste)
        {
            ApplyConfig();

            InitFoundations(foundations);
            InitTableau(tableaus);
            InitFreeCells(freeCells);
            if (stock) InitStock();
            if (waste) InitWaste();
        }

        private void InitFoundations(int amount)
        {
            Foundations = new Foundation[amount];
            for (int i = 0; i < amount; i++)
            {
                Foundations[i] = new Foundation();
            }
        }

        private void InitTableau(int amount)
        {
            Tableaus = new Stack<Card>[amount];
            for (int i = 0; i < amount; i++)
            {
                Tableaus[i] = new Stack<Card>();
            }
        }

        private void InitFreeCells(int amount)
        {
            FreeCells = new Stack<Card>[amount];
            for (int i = 0; i < amount; i++)
            {
                FreeCells[i] = new Stack<Card>();
            }
        }

        private void InitStock()
        {
            StockPile = new Stack<Card>();
        }

        private void InitWaste()
        {
            WastePile = new Stack<Card>();
        }

        public void LogState()
        {
            if (StockPile != null)
                Debug.Log($"Stock ({StockPile.Count}): {string.Join(" ", StockPile)}");
            //if (StockPile.Count > 0) Debug.Log($"Top card in stock is: {StockPile.Peek()}");

            if (WastePile != null)
                Debug.Log($"Waste ({WastePile.Count}): {string.Join(" ", WastePile)}");
            //if (WastePile.Count > 0) Debug.Log($"Top card in waste is: {WastePile.Peek()}");

            if (FreeCells != null)
            {
                Debug.Log("FreeCells:");
                for (int i = 0; i < FreeCells.Length; i++)
                {
                    Debug.Log($"  FC{i}: {string.Join(" ", FreeCells[i])}");
                    if (FreeCells[i] != null) Debug.Log($"Card in FC{i} is: {FreeCells[i]}");
                }
            }

            if (Foundations != null)
            {
                Debug.Log("Foundations:");
                for (int i = 0; i < Foundations.Length; i++)
                {
                    Debug.Log($"  F{i}: {string.Join(" ", Foundations[i].Stack)}");
                    if (Foundations[i].Stack.Count > 0) Debug.Log($"Top card in F{i} is: {Foundations[i].Stack.Peek()}");
                }
            }

            if (Tableaus != null)
            {
                Debug.Log("Tableaus:");
                for (int i = 0; i < Tableaus.Length; i++)
                {
                    Debug.Log($"  T{i}: {string.Join(" ", Tableaus[i])}");
                    if (Tableaus[i].Count > 0) Debug.Log($"Top card in T{i} is: {Tableaus[i].Peek()}");
                }
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
                for (int i = 0; i < FreeCells.Length; i++)
                {
                    if (FreeCells[i].Contains(card))
                    {
                        kind = PileKind.FREECELL;
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

        public Stack<Card> GetCardStack(PileKind kind, int index = -1)
        {
            switch (kind)
            {
                case PileKind.WASTE:
                    return WastePile;
                case PileKind.STOCK:
                    return StockPile;
                case PileKind.FOUNDATION:
                    return Foundations[index].Stack;
                case PileKind.TABLEAU:
                    return Tableaus[index];
                case PileKind.FREECELL:
                    return FreeCells[index];
                default:
                    throw new Exception($"[Game] Invalid pile kind {kind}");
            }
        }
    }
}