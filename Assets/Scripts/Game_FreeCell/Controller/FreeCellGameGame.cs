using System.Collections.Generic;
using System.Linq;
using Common;
using Utils;

namespace FreeCell
{
    public class FreeCellGameGame : Game
    {
        public override GameState State => _state;
        public override int FoundationsAmount => 4;
        public override int TableausAmount => 8;
        public override int FreeCellsAmount => 4;
        public override bool HasStock => false;
        public override bool HasWaste => false;
        protected override string DebugTag => "FreeCell";
        protected override int DrawCount => 0;

        public FreeCellGameState FCState => _state;
        private Dictionary<Card, int> _cardStackSizeCache = new();
        private FreeCellGameState _state;

        #region Initialization
        public FreeCellGameGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            RegisterToEvents();
            ShuffleAndDealDeck(deck);
            Log();
        }

        private void RegisterToEvents()
        {
            EventManager.OnStateChanged += OnStateChanged;
        }

        ~FreeCellGameGame()
        {
            EventManager.OnStateChanged -= OnStateChanged;
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

            CommonInner_UpdateFreeCards();
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
                    bool hasRoom = HasRoomToMove(card, sourcePileData, toEmptyTableau);

                    bool validMove = !toEmptyTableau && validStack;
                    bool hasSpaceToMove = !fromTableau || hasRoom;
                    return hasSpaceToMove && (toEmptyTableau || validMove);
                case PileKind.FREECELL:
                    bool cellEmpty = _state.FreeCells[targetPileIndex].Count == 0;
                    return cellEmpty && !tableauCardStackParent;
            }
            return false;
        }
        #endregion

        #region Utilities

        private bool HasRoomToMove(Card card, PileData sourcePileData, bool toEmpty)
        {
            int stackSize = GetMovingStackSize(card, sourcePileData);
            int availableSpace = _state.GetFreeMovingSpaces() + (toEmpty ? 0 : 1);
            return stackSize <= availableSpace;
        }

        private int GetMovingStackSize(Card card, PileData sourcePileData)
        {
            if (sourcePileData.Kind != PileKind.TABLEAU) return 1;

            if (!_cardStackSizeCache.ContainsKey(card))
            {
                int amount = 0;
                bool count = false;
                for (int i = _state.Tableaus[sourcePileData.Index].Count - 1; i >= 0; i--)
                {
                    if (_state.Tableaus[sourcePileData.Index].ElementAt(i) == card)
                    {
                        count = true;
                    }
                    if (count) amount++;
                }
                _cardStackSizeCache.Add(card, amount);
            }

            return _cardStackSizeCache[card];
        }

        private void OnStateChanged()
        {
            _cardStackSizeCache.Clear();
        }
        #endregion
    }
}