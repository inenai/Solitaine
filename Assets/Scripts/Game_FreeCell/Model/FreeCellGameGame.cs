using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

namespace FreeCell
{
    public class FreeCellGameGame : Game
    {
        public override IGameState State => _state;
        public FreeCellGameState FCState => _state;

        protected override string DebugTag => "FreeCell";

        FreeCellGameState _state;

        public FreeCellGameGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            _state = new FreeCellGameState();
            ShuffleAndDealDeck(deck);
            Log();
        }

        #region Initialization

        private void ShuffleAndDealDeck(List<Card> deck)
        {
            Log("Shuffling and dealing...");
            Stack<Card> deckStack = new Stack<Card>(CommonUtils.Shuffle(deck.ToArray()));
            Deal(deckStack);
        }

        private void Deal(Stack<Card> deckStack)
        {
            int tableauIndex = 0;
            while (deckStack.Count > 0)
            {
                Card nextCard = deckStack.Pop();
                nextCard.Show(true);
                _state.Tableaus[tableauIndex].Push(nextCard);
                tableauIndex = (tableauIndex + 1) % 8;
            }
            UpdateFreeCards();
        }
        #endregion

        private void UpdateFreeCards()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < _state.Tableaus[i].Count; j++)
                {
                    if (j == 0)
                        _state.Tableaus[i].ElementAt(j).FreeCard(true);
                    else
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

        #region Checks
        private bool ValidTableauCardStack(Card child, Card parent)
        {
            if (child == null || parent == null) return false;

            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        public bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            PileData sourcePileData = State.GetCardPileOwnerData(card);
            if (sourcePileData.Kind == targetPile && sourcePileData.Index == targetPileIndex)
                return false;

            bool tableauCardStackParent = sourcePileData.Kind == PileKind.TABLEAU
                                       && _state.Tableaus[sourcePileData.Index].Peek() != card;

            switch (targetPile)
            {
                case PileKind.FOUNDATION:
                    bool first = card.Value == 1
                        && _state.Foundations[targetPileIndex].Stack.Count == 0;
                    bool next = card.Value > 1
                        && _state.Foundations[targetPileIndex].Stack.Count > 0
                        && _state.Foundations[targetPileIndex].Suit == card.Suit
                        && _state.Foundations[targetPileIndex].Stack.Peek().Value == card.Value - 1;
                    return !tableauCardStackParent && (first || next);
                case PileKind.TABLEAU:
                    bool toEmptyTableau = _state.Tableaus[targetPileIndex].Count == 0;

                    bool validStack = !toEmptyTableau && ValidTableauCardStack(card, _state.Tableaus[targetPileIndex].Peek());
                    bool fromTableau = sourcePileData.Kind == PileKind.TABLEAU;
                    bool hasRoom = GetMovingStackSize(card, sourcePileData) <= _state.FreeMovingSpaces + (toEmptyTableau ? 0 : 1);

                    bool validMove = !toEmptyTableau && validStack;
                    bool hasSpaceToMove = !fromTableau || hasRoom;
                    Log($"Can add card {card} to tableau? {hasSpaceToMove && (toEmptyTableau || validMove)}. toEmptyTableau {toEmptyTableau}, validStack {validStack}, fromTableau {fromTableau}, hasRoom {hasRoom}, validMove {validMove}, hasSpaceToMove {hasSpaceToMove}, should be hasSpaceToMove && (toEmptyTableau || validMove)");
                    return hasSpaceToMove && (toEmptyTableau || validMove);
                case PileKind.FREECELL:
                    bool cellEmpty = _state.FreeCells[targetPileIndex].Count == 0;
                    return cellEmpty && !tableauCardStackParent;
            }
            return false;
        }

        private int GetMovingStackSize(Card card, PileData pileData)
        {
            if (pileData.Kind != PileKind.TABLEAU) return 1;

            int amount = 0;
            bool count = false;
            for (int i = _state.Tableaus[pileData.Index].Count - 1; i >= 0; i--)
            {
                if (_state.Tableaus[pileData.Index].ElementAt(i) == card)
                {
                    count = true;
                }
                if (count) amount++;
            }
            //Debug.Log($"Moving stack size for card {card} in tableau {pileData.Index} is {amount}");
            return amount;
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

        public Card GetSolvableCard()
        {
            Debug.Log("Looking for automatic move");

            Card card;

            for (int i = 0; i < 4; i++)
            {
                _state.FreeCells[i].TryPeek(out card);
                if (card != null && IsSafeToMoveCardToFoundation(card))
                {
                    Debug.Log($"Safe to move {card} to foundation. Can move?");
                    if (CanMoveCardToAnyFoundation(card))
                    {
                        Debug.Log($"{card} can be moved from free cell[{i}] to foundation.");

                        return card;
                    }
                }
            }

            for (int i = 0; i < 8; i++)
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

        public List<PileKind> Action_TryMoveCardAutomatic(Card card)
        {
            Log($"USER Action_TryMoveCardAutomatic {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            PileKind targetPileKind = default;

            int targetIndexExclude = sourcePileData.Kind == PileKind.TABLEAU ? sourcePileData.Index : -1;

            ExecuteAction(() =>
            {
                bool cardMoved = TryMoveCardToAnyTableau(card, targetIndexExclude);

                if (cardMoved)
                {
                    targetPileKind = PileKind.TABLEAU;
                }
                else
                {
                    cardMoved = TryMoveCardToAnyFreeCell(card);
                    if (cardMoved)
                    {
                        targetPileKind = PileKind.FREECELL;
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

        private bool TryMoveCardToAnyFreeCell(Card card)
        {
            for (int i = 0; i < _state.FreeCells.Length; i++)
            {
                if (_state.FreeCells[i].Count > 0)
                    continue;

                if (TryMoveCardToFreeCellIndex(card, i))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryMoveCardToFreeCellIndex(Card card, int index)
        {
            Log($"INNER TryMoveCardToFreeCellIndex{card} > {index}]");
            if (CanAddCardToPile(card, PileKind.FREECELL, index))
            {
                RemoveCardFromPile(card);
                _state.FreeCells[index].Push(card);
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
                    case PileKind.FREECELL:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                }
                return true;
            }
            return false;
        }

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
                    case PileKind.FREECELL:
                        cardMoved = TryMoveCardToFreeCellIndex(card,targetPileIndex);
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

        private void RemoveCardFromPile(Card card)
        {
            Log($"INNER RemoveCardFromPile {card}");
            PileData pileData = _state.GetCardPileOwnerData(card);
            switch (pileData.Kind)
            {
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
                case PileKind.FREECELL:
                    _state.FreeCells[pileData.Index].Pop();
                    break;
            }
        }

    }
}