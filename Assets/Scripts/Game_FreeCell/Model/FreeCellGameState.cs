using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace FreeCell
{
    public class FreeCellGameState : IGameState
    {
        public Stack<Card>[] FreeCells;
        public Stack<Card>[] Tableaus;
        public Foundation[] Foundations;

        public int FreeMovingSpaces => FreeCells.Count(card => card.Count == 0) + Tableaus.Count(card => card.Count == 0);

        public FreeCellGameState()
        {
            InitFoundations();
            InitTableau();
            InitFreeCells();
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
            Tableaus = new Stack<Card>[8];
            for (int i = 0; i < 8; i++)
            {
                Tableaus[i] = new Stack<Card>();
            }
        }

        private void InitFreeCells()
        {
            FreeCells = new Stack<Card>[4];
            for (int i = 0; i < 4; i++)
            {
                FreeCells[i] = new Stack<Card>();
            }
        }

        public PileData GetCardPileOwnerData(Card card)
        {
            PileKind kind = PileKind.FOUNDATION;
            int index = -1;
            bool found = false;

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

        public void LogState()
        {
            Debug.Log("FreeCells:");
            for (int i = 0; i < FreeCells.Length; i++)
            {
                Debug.Log($"  FC{i}: {string.Join(" ", FreeCells[i])}");
                if (FreeCells[i] != null) Debug.Log($"Card in FC{i} is: {FreeCells[i]}");
            }

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
    }
}