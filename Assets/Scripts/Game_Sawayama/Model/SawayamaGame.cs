using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

namespace Sawayama
{
    public class SawayamaGame : Game
    {
        public override IGameState State => _state;
        public SawayamaState SState => _state;
        protected override string DebugTag => "Sawayama";
        public int DrawCount => 3;
        private bool _drewAllCardsFromStock;

        SawayamaState _state;

        #region initialization
        public SawayamaGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            _state = new SawayamaState();
            ShuffleAndDealDeck(deck);
            Log();
        }

        public static List<Card> CreateGameDeck()
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
            return deck;
        }

        private void ShuffleAndDealDeck(List<Card> deck)
        {
            Log("Shuffling and dealing...");
            Stack<Card> deckStack = new Stack<Card>(CommonUtils.Shuffle(deck.ToArray()));
            _state.StockPile = deckStack;
            Deal();
        }

        private void Deal()
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    Card nextCard = _state.StockPile.Pop();
                    nextCard.Show(true);
                    if (i == j)
                    {
                        nextCard.FreeCard(true);
                    }
                    _state.Tableaus[i].Push(nextCard);
                }
            }
            UpdateFreeCards();
        }

        private void UpdateFreeCards()
        {
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < _state.Tableaus[i].Count; j++)
                {
                    if (_state.Tableaus[i].ElementAt(j).Free) continue;
                    if (j > 0)
                    {
                        if (ValidTableauCardStack(_state.Tableaus[i].ElementAt(j - 1), _state.Tableaus[i].ElementAt(j)))
                        {
                            _state.Tableaus[i].ElementAt(j).FreeCard(true);
                        }
                        else break;
                    }
                }
            }
        }
        #endregion

        protected override bool Won()
        {
            int total = 0;
            foreach (Foundation f in _state.Foundations)
            {
                total += f.Stack.Count;
            }
            return total == 13 * 4;
        }

        #region UserInteraction
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
        /// <param name="targetPileKind"></param>
        /// <param name="targetPileIndex"></param>
        /// <returns><para>A list of the pile kinds that have been affected and should be updated in the ui.</para>
        /// <para>If returned list is not empty you must call UIDoneRefreshing when UI has finished updating. </para></returns>
        public List<PileKind> Action_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Log($"USER Action_DragCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            ExecuteAction(() =>
            {
                bool cardMoved = false;
                switch (targetPileKind)
                {
                    case PileKind.FOUNDATION:
                        cardMoved = TryMoveCardToFoundationIndex(card, targetPileIndex);
                        break;
                    case PileKind.TABLEAU:
                        cardMoved = TryMoveCardsToTableauIndex(card, targetPileIndex);
                        break;
                    case PileKind.STOCK:
                        cardMoved = TryMoveCardToStock(card);
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

        public List<PileKind> Action_TryMoveCardAutomatic(Card card)
        {
            Log($"USER Action_TryMoveCardAutomatic {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            PileKind targetPileKind = default;

            ExecuteAction(() =>
            {
                bool cardMoved = TryMoveCardToAnyTableau(card, sourcePileData.Index);

                if (cardMoved)
                {
                    targetPileKind = PileKind.TABLEAU;
                }
                else
                {
                    cardMoved = TryMoveCardToStock(card);
                    if (cardMoved)
                    {
                        targetPileKind = PileKind.STOCK;
                    }
                    else
                    {
                        cardMoved = TryMoveCardToAnyFoundation(card);
                        if (cardMoved)
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                    }
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

        #region AutomaticActions
        public List<PileKind> Auto_TryMoveCardToFoundationAutomatic(Card card)
        {
            Log($"INNER MoveCardAutomatically {card}");
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            List<PileKind> affectedPiles = new();
            ExecuteAction(() =>
            {
                if (TryMoveCardToAnyFoundation(card))
                {
                    affectedPiles.Add(PileKind.FOUNDATION);
                    affectedPiles.Add(sourcePileData.Kind);
                    return true;
                }
                return false;
            });
            return affectedPiles;
        }
        #endregion

        #region InnerActions
        // DEAL
        private bool TryDrawCardsFromStock()
        {
            Log("INNER TryDrawCardsFromStock");
            if (_drewAllCardsFromStock || _state.StockPile.Count == 0)
            {
                return false;
            }

            if (_state.WastePile.TryPeek(out var topCard))
                topCard.FreeCard(false);

            for (int i = 0; i < DrawCount; i++)
            {
                if (_state.StockPile.Count > 0)
                {
                    Card nextCard = _state.StockPile.Pop();
                    nextCard.Show(true);
                    _state.WastePile.Push(nextCard);
                }
                else break;
            }

            if (_state.StockPile.Count == 0)
            {
                _drewAllCardsFromStock = true;
                EventManager.OnStockEmpty?.Invoke();
            }

            if (_state.WastePile.TryPeek(out topCard))
                topCard.FreeCard(true);

            return true;
        }


        //ADD TO ANY
        private bool TryMoveCardToAnyFoundation(Card card)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (card.Value > 1 && _state.Foundations[i].Suit != card.Suit)
                    continue;

                if (TryMoveCardToFoundationIndex(card, i))
                {
                    return true;
                }
            }
            return false;
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
                card.FreeCard(false);
                return true;
            }
            return false;
        }

        private bool TryMoveCardToAnyTableau(Card card, int excludeIndex = -1)
        {
            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            bool success = false;

            List<int> emptyCandidates = new();
            List<int> compatibleFullCandidates = new();

            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                if (i == excludeIndex) continue;

                if (CanAddCardToPile(card, PileKind.TABLEAU, i))
                {
                    if (_state.Tableaus[i].Count > 0)
                    {
                        compatibleFullCandidates.Add(i);
                    }
                    else
                    {
                        emptyCandidates.Add(i);
                    }
                }
            }

            for (int i = 0; i < compatibleFullCandidates.Count; i++)
            {
                success = TryMoveCardsToTableauIndex(card, compatibleFullCandidates[i]);
                if (success) break;
            }

            if (!success)
            {
                for (int i = 0; i < emptyCandidates.Count; i++)
                {
                    success = TryMoveCardsToTableauIndex(card, emptyCandidates[i]);
                    if (success) break;
                }
            }

            return success;
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
                            UpdateFreeCards();
                        }
                        while (tempStack.Count > 0)
                        {
                            _state.Tableaus[index].Push(tempStack.Pop());
                        }
                        break;
                    case PileKind.WASTE:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                    case PileKind.STOCK:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                }
                return true;
            }
            return false;
        }

        private bool TryMoveCardToStock(Card card)
        {
            if (CanAddCardToPile(card, PileKind.STOCK, -1))
            {
                RemoveCardFromPile(card);
                _state.StockPile.Push(card);
                return true;
            }
            return false;
        }

        // REMOVE
        private void RemoveCardFromPile(Card card)
        {
            Log($"INNER RemoveCardFromPile {card}");
            PileData pileData = _state.GetCardPileOwnerData(card);
            switch (pileData.Kind)
            {
                case PileKind.WASTE:
                    _state.WastePile.Pop();
                    if (_state.WastePile.Count > 0)
                    {
                        _state.WastePile.Peek().FreeCard(true);
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
                            UpdateFreeCards();
                        }
                    }
                    break;
                case PileKind.STOCK:
                    _state.StockPile.Pop();
                    break;
            }
        }
        #endregion

        #region Checks
        private bool ValidTableauCardStack(Card child, Card parent)
        {
            if (child == null || parent == null) return false;
            //Debug.Log($"Valid Tableau Stack {child} > {parent}?");
            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        private bool CanMoveCardToAnyFoundation(Card card)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");
            if (card.Value == 1) return true;

            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (_state.Foundations[i].Suit != card.Suit)
                    continue;

                return CanAddCardToPile(card, PileKind.FOUNDATION, i);
            }
            return false;
        }

        public bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            PileData sourcePileData = State.GetCardPileOwnerData(card);
            if (sourcePileData.Kind == targetPile && sourcePileData.Index == targetPileIndex)
                return false;
            if (sourcePileData.Kind == PileKind.FOUNDATION)
                return false;

            bool tableauCardStackParent = sourcePileData.Kind == PileKind.TABLEAU
                                       && _state.Tableaus[sourcePileData.Index].Peek() != card;

            switch (targetPile)
            {
                case PileKind.STOCK:
                    bool stockEmpty = _state.StockPile.Count == 0;
                    return stockEmpty && !tableauCardStackParent;
                case PileKind.WASTE:
                    return false;
                case PileKind.FOUNDATION:
                    bool first = card.Value == 1
                        && _state.Foundations[targetPileIndex].Stack.Count == 0;
                    bool next = card.Value > 1
                        && _state.Foundations[targetPileIndex].Stack.Count > 0
                        && _state.Foundations[targetPileIndex].Suit == card.Suit
                        && _state.Foundations[targetPileIndex].Stack.Peek().Value == card.Value - 1;
                    return !tableauCardStackParent && (first || next);
                case PileKind.TABLEAU:
                    bool toEmpty = _state.Tableaus[targetPileIndex].Count == 0;
                    bool validMove = _state.Tableaus[targetPileIndex].Count > 0 &&
                        ValidTableauCardStack(card, _state.Tableaus[targetPileIndex].Peek());
                    return toEmpty || validMove;
            }
            return false;
        }

        private bool IsSafeToMoveCardToFoundation(Card card)
        {
            if (card.Value < 3) return true;
            CardSuit[] oppositeColorSuites = CardUtils.GetOppositeColorSuits(card.Suit);

            bool card1Found = false;
            foreach (Foundation f in _state.Foundations)
            {
                if (f.Suit == oppositeColorSuites[0])
                {
                    foreach (Card c in f.Stack)
                    {
                        if (c.Value == card.Value - 2)
                        {
                            card1Found = true;
                            break;
                        }
                    }
                }
            }
            if (!card1Found) return false;

            bool card2found = false;
            foreach (Foundation f in _state.Foundations)
            {
                if (f.Suit == oppositeColorSuites[1])
                {
                    foreach (Card c in f.Stack)
                    {
                        if (c.Value == card.Value - 2)
                        {
                            card2found = true;
                            break;
                        }
                    }
                }
            }
            return card2found;
        }

        public Card GetSolvableCard()
        {
            Debug.Log("Looking for automatic move");

            Card card;
            _state.WastePile.TryPeek(out card);

            if (card != null && IsSafeToMoveCardToFoundation(card))
            {
                Debug.Log($"Safe to move {card} to foundation. Can move?");
                if (CanMoveCardToAnyFoundation(card))
                {
                    Debug.Log($"{card} can be moved from waste to foundation.");
                    return card;
                }
            }

            for (int i = 0; i < 7; i++)
            {
                _state.Tableaus[i].TryPeek(out card);
                if (card != null && IsSafeToMoveCardToFoundation(card))
                {
                    Debug.Log($"Safe to move {card} to foundation. Can move?");
                    if (CanMoveCardToAnyFoundation(card))
                    {
                        Debug.Log($"{card} can be moved from tableau[{i}] to foundation.");

                        return card;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}