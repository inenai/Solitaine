using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;
using Utils;

namespace Sawayama
{
    public class SawayamaGame : Game
    {
        public override GameState State => _state;
        public SawayamaState SState => _state;
        protected override string DebugTag => "Sawayama";
        public override int FoundationsAmount => 4;
        public override int TableausAmount => 7;
        public override int FreeCellsAmount => 1;
        public override bool HasStock => true;
        public override bool HasWaste => true;
        protected override int DrawCount => 3;

        SawayamaState _state;

        #region initialization
        public SawayamaGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            ShuffleAndDealDeck(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new SawayamaState(this);
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
            CommonInner_UpdateFreeCards();
        }
        #endregion

        #region Checks
        protected override bool ValidTableauCardStack(Card child, Card parent)
        {
            if (child == null || parent == null) return false;
            //Debug.Log($"Valid Tableau Stack {child} > {parent}?");
            bool sameColor = CardUtils.IsSameColor(child.Suit, parent.Suit);
            return !sameColor && child.Value == parent.Value - 1;
        }

        private bool CanMoveCardToAnyFoundation(Card card)
        {
            Log($"INNER CanMoveCardToAnyFoundation {card} > F*");
            if (card.Value == 1) return true;

            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (_state.Foundations[i].Suit != card.Suit)
                    continue;

                return CanAddCardToPile(card, PileKind.FOUNDATION, i);
            }
            return false;
        }

        public override bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
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
                case PileKind.FREECELL:
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
        #endregion

    }
}