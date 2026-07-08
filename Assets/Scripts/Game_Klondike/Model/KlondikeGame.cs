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
        public KlondikeGame()
        {
            _state = new KlondikeState();
            _state.InitState();
        }

        public void SetupGame()
        {
            Log("Starting a Klondike game.");
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
                    _state.Tableaus[i].Push(nextCard);
                }
            }

            _state.StockPile = deck;
            _state.OnRestocked();
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

        #region GameFlow
        public void UIDoneRefreshing()
        {
            Status = GameStatus.LISTENING;
        }
        #endregion

        #region userInteraction

        /// <summary>
        /// Command Wrapper!
        /// </summary>
        /// <param name="action"></param>
        /// <returns>Whether a UI refresh is needed</returns>
        private bool ExecuteAction(Func<bool> action)
        {
            if (Status != GameStatus.LISTENING)
                return false;

            Status = GameStatus.PROCESSING;

            bool uiRefreshNeeded = action();

            if (uiRefreshNeeded)
            {
                Log();
            }
            else
            {
                Status = GameStatus.LISTENING;
            }
            return uiRefreshNeeded;
        }

        public List<PileKind> Action_TryDrawCardsFromStock()
        {
            List<PileKind> affectedPiles = new List<PileKind>();
            bool uiRefreshNeeded = ExecuteAction(() =>
            {
                return TryDrawCardsFromStock();
            });

            if (uiRefreshNeeded)
            {
                affectedPiles.Add(PileKind.WASTE);
                affectedPiles.Add(PileKind.STOCK);
            }
            return affectedPiles;
        }

        /// <summary>
        /// </summary>
        /// <param name="card"></param>
        /// <returns><para>A list of the pile kinds that have been affected and should be updated in the ui.</para>
        /// <para>If returned list is not empty you must call UIDoneRefreshing when UI has finished updating. </para></returns>
        public List<PileKind> Action_TryMoveCardAutomatic(Card card)
        {
            Log($"USER Action_TryMoveCardAutomatic {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            PileKind targetPileKind = default;

            bool uiRefreshPending = ExecuteAction(() =>
            {
                bool cardMoved = false;

                switch (sourcePileData.Kind)
                {
                    case PileKind.WASTE:
                        cardMoved = TryMoveCardToAnyFoundation(card);
                        if (cardMoved) targetPileKind = PileKind.FOUNDATION;

                        if (!cardMoved)
                        {
                            cardMoved = TryMoveCardToAnyTableau(card);
                            if (cardMoved) targetPileKind = PileKind.TABLEAU;
                        }
                        break;
                    case PileKind.FOUNDATION:
                        cardMoved = TryMoveCardToAnyTableau(card);
                        if (cardMoved) targetPileKind = PileKind.TABLEAU;
                        break;
                    case PileKind.TABLEAU:
                        cardMoved = TryMoveCardToAnyFoundation(card);
                        if (cardMoved) targetPileKind = PileKind.FOUNDATION;
                        if (!cardMoved)
                        {
                            cardMoved = TryMoveCardToAnyTableau(card, sourcePileData.Index);
                            if (cardMoved) targetPileKind = PileKind.TABLEAU;
                        }
                        break;
                }

                if (cardMoved)
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    if (sourcePileData.Kind != targetPileKind)
                        affectedPiles.Add(targetPileKind);
                }

                return cardMoved;
            });

            return affectedPiles;
        }

        /// <summary>
        /// </summary>
        /// <param name="card"></param>
        /// <param name="targetPileKind"></param>
        /// <param name="targetPileIndex"></param>
        /// <returns><para>A list of the pile kinds that have been affected and should be updated in the ui.</para>
        /// <para>If returned list is not empty you must call UIDoneRefreshing when UI has finished updating. </para></returns>
        public List<PileKind> Action_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Log($"USER Action_DragCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            bool uiRefreshPending = ExecuteAction(() =>
            {
                bool cardMoved = false;
                switch (sourcePileData.Kind)
                {
                    case PileKind.WASTE: // > Tableau, Foundations
                        switch (targetPileKind)
                        {
                            case PileKind.FOUNDATION:
                                cardMoved = TryMoveCardToFoundationIndex(card, targetPileIndex);
                                break;
                            case PileKind.TABLEAU:
                                cardMoved = TryMoveCardsToTableauIndex(card, targetPileIndex);
                                break;
                        }
                        break;

                    case PileKind.FOUNDATION:// > Tableau
                        if (State.FoundationCardsFree)
                        {
                            switch (targetPileKind)
                            {
                                case PileKind.TABLEAU:
                                    cardMoved = TryMoveCardsToTableauIndex(card, targetPileIndex);
                                    break;
                            }
                        }
                        break;

                    case PileKind.TABLEAU: // > Tableau, Foundations
                        switch (targetPileKind)
                        {
                            case PileKind.FOUNDATION:
                                cardMoved = TryMoveCardToFoundationIndex(card, targetPileIndex);
                                break;
                            case PileKind.TABLEAU:
                                cardMoved = TryMoveCardsToTableauIndex(card, targetPileIndex);
                                break;
                        }
                        break;
                }

                if (cardMoved)
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    if (sourcePileData.Kind != targetPileKind)
                        affectedPiles.Add(targetPileKind);
                }

                return cardMoved;
            });

            return affectedPiles;
        }
        #endregion


        #region innerActions

        // DEAL
        private bool TryDrawCardsFromStock()
        {
            Log("INNER TryDrawCardsFromStock");
            if (_state.StockPile.Count == 0)
            {
                return TryRestock();
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
        }

        private bool TryRestock()
        {
            Log("INNER AttemptRestock");
            if (_state.WastePile.Count > 0 && _state.AvailableRestocks != 0)
            {
                while (_state.WastePile.Count > 0)
                {
                    Card nextCard = _state.WastePile.Pop();
                    nextCard.Show(false);
                    _state.StockPile.Push(nextCard);
                }
                _state.OnRestocked();
                return true;
            }
            return false;
        }

        //ADD TO ANY
        private bool TryMoveCardToAnyFoundation(Card card)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (TryMoveCardToFoundationIndex(card, i))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryMoveCardToAnyTableau(Card card, int excludeIndex = -1)
        {
            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            bool success = false;
            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                if (i == excludeIndex) continue;

                success = TryMoveCardsToTableauIndex(card, i);
                if (success) break;
            }
            return success;
        }

        //ADD TO INDEX
        private bool TryMoveCardToFoundationIndex(Card card, int index)
        {
            Foundation foundation = _state.Foundations[index];
            Log($"INNER TryMoveCardToFoundationIndex{card} > {foundation}]");
            if (CanAddCardToPile(card, PileKind.FOUNDATION, index))
            {
                RemoveCardFromPile(card);
                if (foundation.Stack.Count == 0)
                    foundation.Suit = card.Suit;
                else
                    foundation.Stack.Peek().FreeCard(false);
                foundation.Stack.Push(card);
                card.FreeCard(_state.FoundationCardsFree);
                return true;
            }
            return false;
        }

        private bool TryMoveCardsToTableauIndex(Card card, int index)
        {
            Log($"INNER TryMoveCardsToTableauIndex {card} > T[{index}]");
            if (CanAddCardToPile(card, PileKind.TABLEAU, index)) //VALIDATION DONE
            {
                PileData sourcePileData = _state.GetCardPileOwnerData(card);
                switch (sourcePileData.Kind)
                {
                    case PileKind.TABLEAU:
                        Stack<Card> fromTableau = _state.Tableaus[sourcePileData.Index];

                        Stack<Card> tempStack = new Stack<Card>();
                        while (tempStack.Count == 0 || tempStack.Peek() != card)
                        {
                            tempStack.Push(fromTableau.Pop());
                        }
                        if (fromTableau.Count > 0)
                        {
                            fromTableau.Peek().Show(true);
                            fromTableau.Peek().FreeCard(true);
                        }
                        while (tempStack.Count > 0)
                        {
                            _state.Tableaus[index].Push(tempStack.Pop());
                        }
                        break;
                    case PileKind.FOUNDATION:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                    case PileKind.WASTE:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                }
                return true;
            }
            return false;
        }

        // REMOVE
        private void RemoveCardFromPile(Card card)
        {
            Log($"INNER RemoveCardFromPile {card}");
            PileData pileData = State.GetCardPileOwnerData(card);
            switch (pileData.Kind)
            {
                case PileKind.WASTE:
                    _state.WastePile.Pop();
                    if (_state.WastePile.Count > 0)
                    {
                        _state.WastePile.Peek().FreeCard(true);
                    }
                    break;
                case PileKind.FOUNDATION:
                    _state.Foundations[pileData.Index].Stack.Pop();
                    if (_state.FoundationCardsFree && _state.Foundations[pileData.Index].Stack.Count > 0)
                    {
                        _state.Foundations[pileData.Index].Stack.Peek().FreeCard(true);
                    }
                    break;
                case PileKind.TABLEAU:
                    if (_state.Tableaus[pileData.Index].Peek() == card)
                    {
                        _state.Tableaus[pileData.Index].Pop();
                        if (_state.Tableaus[pileData.Index].Count > 0)
                        {
                            _state.Tableaus[pileData.Index].Peek().FreeCard(true);
                            _state.Tableaus[pileData.Index].Peek().Show(true);
                        }
                    }
                    break;
            }
        }
        #endregion

        #region Checks
        private bool ValidTableauCardStack(Card child, Card parent)
        {
            if (child == null || parent == null) return false;

            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        public bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            switch (targetPile)
            {
                case PileKind.STOCK:
                    return false;
                case PileKind.WASTE:
                    return false;
                case PileKind.FOUNDATION:
                    bool first = card.Value == 1
                        && _state.Foundations[targetPileIndex].Stack.Count == 0;
                    bool next = card.Value > 1
                        && _state.Foundations[targetPileIndex].Stack.Count > 0
                        && _state.Foundations[targetPileIndex].Suit == card.Suit
                        && _state.Foundations[targetPileIndex].Stack.Peek().Value == card.Value - 1;
                    return first || next;
                case PileKind.TABLEAU:
                    bool kingToEmpty = _state.Tableaus[targetPileIndex].Count == 0
                        && card.Value == 13;
                    bool validMove = _state.Tableaus[targetPileIndex].Count > 0 &&
                        ValidTableauCardStack(card, _state.Tableaus[targetPileIndex].Peek());
                    return kingToEmpty || validMove;
            }
            return false;
        }
        #endregion

        #region utils
        public void Log()
        {
            Debug.Log("=== SOLITAIRE STATE ===");

            Debug.Log($"Stock ({_state.StockPile.Count}): {string.Join(" ", _state.StockPile)}");
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
            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                Debug.Log($"  T{i}: {string.Join(" ", _state.Tableaus[i])}");
                if (_state.Tableaus[i].Count > 0) Debug.Log($"Top card in T{i} is: {_state.Tableaus[i].Peek()}");
            }
        }

        private void Log(string message)
        {
            Debug.Log($"[Klondike] {message}");
        }
        #endregion
    }
}