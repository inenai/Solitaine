using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public abstract class GameState
    {
        public CardPile StockPile;
        public CardPile WastePile;
        public CardPile[] FreeCells;
        public Tableau[] Tableaus;
        public Foundation[] Foundations;

        Stack<GameCommand> _doneMoves;
        Stack<GameCommand> _undoneMoves;

        public abstract int AvailableRestocks { get; }
        public abstract bool FoundationCardsFree { get; }
        public bool RedoAvailable => _undoneMoves?.Count > 0;
        public bool UndoAvailable => _doneMoves?.Count > 0;

        public void OnWin()
        {
            foreach (CardPile f in Foundations)
            {
                foreach (Card card in f)
                {
                    card.FreeCard(false);
                }
            }
        }
        protected virtual void ApplyConfig(){}
        public virtual void OnRestock(bool undo = false){}

        public GameState(Game game)
        {
            ApplyConfig();

            InitFoundations(game.FoundationsAmount);
            InitTableau(game.TableausAmount);
            InitFreeCells(game.FreeCellsAmount);
            if (game.HasStock) InitStock();
            if (game.HasWaste) InitWaste();
        }

        public void ResetSavedMoves()
        {
            _doneMoves = new();
            _undoneMoves = new();
        }

        public void SaveCommand(GameCommand command)
        {
            Debug.Log("SAVING COMMAND");
            if (_doneMoves == null) _doneMoves = new();
            _undoneMoves.Clear();
            _doneMoves.Push(command);
            EventManager.OnStateChanged?.Invoke();
        }

        public bool UndoCommand()
        {
            if (_doneMoves.Count == 0) return false;
            GameCommand c = _doneMoves.Pop();
            foreach (GameCommandAction a in c.Actions)
                a.Execute(this, undo: true);
            _undoneMoves.Push(c);
            EventManager.OnStateChanged?.Invoke();
            return true;
        }

        public bool RedoCommand()
        {
            if (_undoneMoves.Count == 0) return false;
            GameCommand c = _undoneMoves.Pop();
            foreach (GameCommandAction a in c.Actions)
                a.Execute(this);
            _doneMoves.Push(c);
            EventManager.OnStateChanged?.Invoke();
            return true;
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
            Tableaus = new Tableau[amount];
            for (int i = 0; i < amount; i++)
            {
                Tableaus[i] = new Tableau();
            }
        }

        private void InitFreeCells(int amount)
        {
            FreeCells = new CardPile[amount];
            for (int i = 0; i < amount; i++)
            {
                FreeCells[i] = new CardPile();
            }
        }

        private void InitStock()
        {
            StockPile = new CardPile();
        }

        private void InitWaste()
        {
            WastePile = new CardPile();
        }

        public void LogState()
        {
            if (StockPile != null)
                Debug.Log($"Stock ({StockPile.Count}): {string.Join(" ", StockPile)}");
            //if (StockPile.Count > 0) Debug.Log($"Top card in stock is: {StockPile.Peek()}");

            if (WastePile != null)
                Debug.Log($"Waste ({WastePile.Count}): {string.Join(" ", WastePile)}");
            //if (WastePile.Count > 0) Debug.Log($"Top card in waste is: {WastePile.Peek()}");

            if (FreeCells != null && FreeCells.Length > 0)
            {
                Debug.Log("FreeCells:");
                for (int i = 0; i < FreeCells.Length; i++)
                {
                    Debug.Log($"  FC{i}: {string.Join(" ", FreeCells[i])}");
                    if (FreeCells[i] != null) Debug.Log($"Card in FC{i} is: {FreeCells[i]}");
                }
            }

            if (Foundations != null && Foundations.Length > 0)
            {
                Debug.Log("Foundations:");
                for (int i = 0; i < Foundations.Length; i++)
                {
                    Debug.Log($"  F{i}: {string.Join(" ", Foundations[i])}");
                    if (Foundations[i].Count > 0) Debug.Log($"Top card in F{i} is: {Foundations[i].Peek()}");
                }
            }

            if (Tableaus != null && Tableaus.Length > 0)
            {
                Debug.Log("Tableaus:");
                for (int i = 0; i < Tableaus.Length; i++)
                {
                    Debug.Log($"  T{i}: {string.Join(" ", Tableaus[i])}");
                    if (Tableaus[i].Count > 0) Debug.Log($"Top card in T{i} is: {Tableaus[i].Peek()}");
                }
            }

            if (_doneMoves != null)
            {
                Debug.Log($"UNDOs available: {_doneMoves.Count}");
            }

            if (_undoneMoves != null)
            {
                Debug.Log($"REDOs available: {_undoneMoves.Count}");
            }
        }


        public PileData GetCardPileOwnerData(Card card)
        {
            PileKind kind = PileKind.WASTE;
            int index = -1;
            bool found = false;

            if (WastePile != null && WastePile.Contains(card))
            {
                kind = PileKind.WASTE;
                found = true;
            }

            if (!found && StockPile != null && StockPile.Contains(card))
            {
                kind = PileKind.STOCK;
                found = true;
            }

            if (!found)
            {
                for (int i = 0; i < Foundations.Length; i++)
                {
                    if (Foundations[i].Contains(card))
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

        public CardPile GetCardStack(PileKind kind, int index = -1)
        {
            switch (kind)
            {
                case PileKind.WASTE:
                    return WastePile;
                case PileKind.STOCK:
                    return StockPile;
                case PileKind.FOUNDATION:
                    return Foundations[index];
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