using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using static Utils.Utils;
using Utils;
using UnityEngine;

namespace Klondike
{
        public class KlondikeGame
    {
        KlondikeState _state;
        GameStatus _status = GameStatus.INITIALIZING;
        GameStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                Log($"STATUS {value}");
            }
        }



        public KlondikeState State => _state;

        #region initialization
        public KlondikeGame(KlondikeController controller)
        {
            _state = new KlondikeState();
            _state.InitState();
        }

        public void SetupGame()
        {
            Log("Starting a Klokdike game.");
            Status = GameStatus.INITIALIZING;
            ShuffleAndDeal();
            Log();
        }

        public void OnUISetup()
        {
            Status = GameStatus.LISTENING;
        }

        private void ShuffleAndDeal()
        {
            Log("Shuffling and dealing...");
            Stack<Card> deck = CreateDeck(shuffle: true);

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    Card nextCard = deck.Pop();
                    if (i == j)
                    {
                        nextCard.Show(true);
                        nextCard.FreeCard(true);
                    }
                    _state.Tableau[i].Push(nextCard);
                }
            }

            _state.StockPile = deck;
        }

        private Stack<Card> CreateDeck(bool shuffle = false)
        {
            List<Card> deck = new();

            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.HEARTS, i));
            }
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.DIAMONDS, i));
            }
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.SPADES, i));
            }
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.CLUBS, i));
            }

            if (shuffle)
            {
                deck = Shuffle(deck.ToArray()).ToList();
            }

            return new Stack<Card>(deck);
        }

        #endregion

        #region userInteraction

        public void UIDoneRefreshing()
        {
            Status = GameStatus.LISTENING;
        }

/// <summary>
/// Command Wrapper!
/// </summary>
/// <param name="action"></param>
/// <returns></returns>
        private bool ExecuteAction(Func<bool> action)
        {
            if (Status != GameStatus.LISTENING)
                return false;

            Status = GameStatus.PROCESSING;

            bool result = action();

            if (result)
            {
                Log();
            }
            else
            {
                Status = GameStatus.LISTENING;
            }

            return result;
        }

        public bool Action_MoveFromTableauToFoundation(int tableauIndex)
        {
            Log($"USER Action_MoveFromTableauToFoundation [{tableauIndex}]");
            return ExecuteAction(() =>
            {
                Card card = _state.Tableau[tableauIndex].Peek();
                if (AutoMoveCardToFoundation(card))
                {
                    _state.Tableau[tableauIndex].Pop();
                    if (_state.Tableau[tableauIndex].Count > 0)
                    {
                        _state.Tableau[tableauIndex].Peek().FreeCard(true);
                    }
                    return true;
                }

                return false;
            });
        }

        public bool Action_DrawCardsFromStock()
        {
            Log($"USER Action_DrawCardsFromStock");
            return ExecuteAction(() =>
            {
                if (_state.StockPile.Count == 0)
                {
                    return AttemptRestock();
                }

                if (_state.WastePile.TryPeek(out var topCard))
                    topCard.FreeCard(false);

                for (int i = 0; i < _state.DrawCount; i++)
                {
                    if (_state.StockPile.Count > 0)
                    {
                        Card nextCard = _state.StockPile.Pop();
                        nextCard.Show(true);
                        _state.WastePile.Push(nextCard);
                    }
                    else break;
                }

                if (_state.WastePile.TryPeek(out topCard))
                    topCard.FreeCard(true);

                return true;
            });
        }


        public bool Action_MoveFromTableauToTableau(int toTableauIndex, Card card)
        {
            Log($"USER Action_MoveFromTableauToTableau [{toTableauIndex}], [{card}]");
            return ExecuteAction(() =>
            {
                CardPileData cardData = _state.GetCardPileOwnerData(card);
                Stack<Card> originPile = _state.Tableau[cardData.Index];
                Stack<Card> targetPile = _state.Tableau[toTableauIndex];

                if (targetPile.Count == 0 && card.Value != 13)
                    return false;

                if (targetPile.Count > 0 && !ValidTableauPlacement(card, targetPile.Peek()))
                    return false;

                Stack<Card> movingStack = new Stack<Card>();

                Card movingCard = originPile.Pop();
                while (movingCard != card)
                {
                    movingStack.Push(movingCard);
                    movingCard = originPile.Pop();
                }

                movingStack.Push(movingCard);
                if (originPile.Count > 0)
                {
                    originPile.Peek().FreeCard(true);
                }

                while (movingStack.Count > 0)
                {
                    targetPile.Push(movingStack.Pop());
                }

                return true;
            });
        }

        public bool Action_MoveFromFoundationToTableau(int foundationIndex, int toTableauIndex)
        {
            Log($"USER Action_MoveFromFoundationToTableau [{foundationIndex}] > [{toTableauIndex}]");
            return ExecuteAction(() =>
            {
                Stack<Card> originPile = _state.Foundations[foundationIndex].Stack;

                if (originPile.Count < 1)
                    return false;

                var targetPile = _state.Tableau[toTableauIndex];

                if (targetPile.Count < 1)
                    return originPile.Peek().Value == 13;

                if (ValidTableauPlacement(originPile.Peek(), targetPile.Peek()))
                {
                    targetPile.Push(originPile.Pop());
                    return true;
                }

                return false;
            });
        }

        public bool Action_MoveFromWasteToTableau(int toTableauIndex)
        {
            Log($"USER Action_MoveFromFoundationToTableau [{toTableauIndex}]");
            return ExecuteAction(() =>
            {
                if (_state.WastePile.Count < 1)
                    return false;

                Card card = _state.WastePile.Peek();
                Stack<Card> tableau = _state.Tableau[toTableauIndex];

                if (tableau.Count < 1)
                    return card.Value == 13;

                return ValidTableauPlacement(card, tableau.Peek());
            });
        }

        public bool Action_DragFromWasteToFoundation(int toFoundationIndex)
        {
            Log($"USER Action_MoveFromWasteToFoundation [{toFoundationIndex}]");
            return ExecuteAction(() =>
            {
                if (_state.WastePile.Count < 1)
                    return false;

                Foundation foundation = _state.Foundations[toFoundationIndex];

                Card card = _state.WastePile.Peek();

                if (foundation.Stack.Count == 0)
                {
                    if (card.Value == 1)
                    {
                        foundation.Suit = card.Suit;
                        foundation.Stack.Push(_state.WastePile.Peek());
                        RemoveTopCardFromWaste();
                        return true;
                    }

                    return false;
                }

                if (card.Suit == foundation.Suit &&
                    card.Value == foundation.Stack.Peek().Value + 1)
                {
                    foundation.Stack.Push(_state.WastePile.Peek());
                    RemoveTopCardFromWaste();
                    return true;
                }

                return false;
            });
        }

        public bool Action_MoveFromWasteToAutoFoundation()
        {
            Log($"USER Action_MoveFromWasteToAutoFoundation");
            return ExecuteAction(() =>
            {
                if (_state.WastePile.Count < 1)
                    return false;

                Card card = _state.WastePile.Peek();

                if (AutoMoveCardToFoundation(card))
                {
                    RemoveTopCardFromWaste();
                    return true;
                }

                return false;
            });
        }
        #endregion


        #region innerActions
        private void RemoveTopCardFromWaste()
        {
            Log($"INNER RemoveTopCardFromWaste");
            _state.WastePile.Pop();
            if (_state.WastePile.Count > 0)
            {
                _state.WastePile.Peek().FreeCard(true);
            }
        }

        private bool AutoMoveCardToFoundation(Card card)
        {
            Log($"INNER AutoMoveCardToFoundation [{card}]");
            if (card.Value == 1)
            {
                foreach (Foundation foundation in _state.Foundations)
                {
                    if (foundation.Stack.Count == 0)
                    {
                        foundation.Suit = card.Suit;
                        foundation.Stack.Push(card);
                        return true;
                    }
                }
            }
            else
            {
                foreach (Foundation foundation in _state.Foundations)
                {
                    if (foundation.Suit == card.Suit)
                    {
                        if (card.Value == foundation.Stack.Peek().Value + 1)
                        {
                            foundation.Stack.Push(card);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool AttemptRestock()
        {
            Log("INNER AttemptRestock");
            if (_state.WastePile.Count > 0 && _state.AllowRestock)
            {
                while (_state.WastePile.Count > 0)
                {
                    Card nextCard = _state.WastePile.Pop();
                    nextCard.Show(false);
                    _state.StockPile.Push(nextCard);
                }
                return true;
            }
            return false;
        }


        #endregion

        #region utils
        private bool ValidTableauPlacement(Card child, Card parent)
        {
            if (child == null || parent == null) return false;

            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        public void Log()
        {
            Debug.Log("=== SOLITAIRE STATE ===");

            Debug.Log($"Waste ({_state.WastePile.Count}): {string.Join(" ", _state.StockPile)}");
            if (_state.StockPile.Count > 0) Debug.Log($"Top card in stock is: {_state.StockPile.Peek()}");
            Debug.Log($"Waste ({_state.WastePile.Count}): {string.Join(" ", _state.WastePile)}");
            if (_state.WastePile.Count > 0) Debug.Log($"Top card in waste is: {_state.WastePile.Peek()}");

            Debug.Log("Foundations:");
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                Debug.Log($"  F{i}: {string.Join(" ", _state.Foundations[i].Stack)}");
                if (_state.Foundations[i].Stack.Count > 0) Debug.Log($"Top card in F{i} is: {_state.Foundations[i].Stack.Peek()}");
            }

            Debug.Log("Tableaus:");
            for (int i = 0; i < _state.Tableau.Length; i++)
            {
                Debug.Log($"  T{i}: {string.Join(" ", _state.Tableau[i])}");
                if (_state.Tableau[i].Count > 0) Debug.Log($"Top card in T{i} is: {_state.Tableau[i].Peek()}");
            }
        }

        private void Log(string message)
        {
            Debug.Log($"[Klondike] {message}");
        }
        #endregion
    }
}