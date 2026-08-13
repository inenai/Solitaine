using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

namespace FreeCell
{
    public class FreeCellGameGame : Game
    {
        public override GameState State => _state;
        public FreeCellGameState FCState => _state;

        protected override string DebugTag => "FreeCell";
        public override int FoundationsAmount => 4;
        public override int TableausAmount => 8;
        public override int FreeCellsAmount => 4;
        public override bool HasStock => false;
        public override bool HasWaste => false;
        protected override int DrawCount => 0;

        FreeCellGameState _state;

        #region Initialization
        public FreeCellGameGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            ShuffleAndDealDeck(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new FreeCellGameState(this);
        }

        private void ShuffleAndDealDeck(List<Card> deck)
        {
            Log("Shuffling and dealing...");
            Stack<Card> deckStack = new Stack<Card>(CommonUtils.Shuffle(deck.ToArray()));
            int tableauIndex = 0;

            while (deckStack.Count > 0)
            {
                Card nextCard = deckStack.Pop();
                nextCard.Show(true);
                _state.Tableaus[tableauIndex].Push(nextCard);
                tableauIndex = (tableauIndex + 1) % 8;
            }

            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                _state.Tableaus[i].Peek().FreeCard(true);
            }
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

        #endregion

        #region Actions
        public override List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex)
        {
            return new List<PileKind>();
        }
        #endregion

        #region Checks
        protected override bool ValidTableauCardStack(Card child, Card parent)
        {
            if (child == null || parent == null) return false;

            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        public override bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
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
        #endregion

        #region Utilities

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
    }
}