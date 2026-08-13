using System.Collections.Generic;
using Common;
using static Utils.CommonUtils;
using Utils;
using UnityEngine;

namespace Klondike
{
    public class KlondikeGame : Game
    {
        public override GameState State => _state;

        public KlondikeState KState => _state;
        protected override string DebugTag => "Klondike";

        public override int FoundationsAmount => 4;
        public override int TableausAmount => 7;
        public override int FreeCellsAmount => 0;
        public override bool HasStock => true;
        public override bool HasWaste => true;
        protected override int DrawCount => KState.DrawCount;

        KlondikeState _state;

        #region initialization
        public KlondikeGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            ShuffleAndDeal(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new KlondikeState(this);
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

        private void ShuffleAndDeal(List<Card> deck)
        {
            Log("Shuffling and dealing...");
            Stack<Card> deckStack = new Stack<Card>(Shuffle(deck.ToArray()));

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    Card nextCard = deckStack.Pop();
                    if (i == j)
                    {
                        nextCard.Show(true);
                        nextCard.FreeCard(true);
                    }
                    _state.Tableaus[i].Push(nextCard);
                }
            }

            _state.StockPile = deckStack;
            _state.OnRestock();
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
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            if (sourcePileData.Kind == targetPile && sourcePileData.Index == targetPileIndex)
                return false;
            if (sourcePileData.Kind == PileKind.FOUNDATION && !_state.FoundationCardsFree)
                return false;

            switch (targetPile)
            {
                case PileKind.STOCK:
                    return false;
                case PileKind.WASTE:
                    return false;
                case PileKind.FOUNDATION:
                    bool tableauCardStackParent = sourcePileData.Kind == PileKind.TABLEAU
                        && _state.Tableaus[sourcePileData.Index].Peek() != card;
                    bool first = card.Value == 1
                        && _state.Foundations[targetPileIndex].Stack.Count == 0;
                    bool next = card.Value > 1
                        && _state.Foundations[targetPileIndex].Stack.Count > 0
                        && _state.Foundations[targetPileIndex].Suit == card.Suit
                        && _state.Foundations[targetPileIndex].Stack.Peek().Value == card.Value - 1;
                    return !tableauCardStackParent && (first || next);
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

    }
}