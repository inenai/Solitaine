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
            UpdateFreeCards();
        }

        private Queue<GameCommandAction> UpdateFreeCards()
        {
            Queue<GameCommandAction> commands = new();
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < _state.Tableaus[i].Count; j++)
                {
                    if (_state.Tableaus[i].ElementAt(j).Free) continue;
                    if (j == 0)
                    {
                        GameCommandAction a = new GameCommandActionFree(
                             _state.Tableaus[i].ElementAt(j),
                             cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        commands.Enqueue(a);
                    }
                    if (j > 0)
                    {
                        if (ValidTableauCardStack(_state.Tableaus[i].ElementAt(j - 1), _state.Tableaus[i].ElementAt(j)))
                        {
                            GameCommandAction a = new GameCommandActionFree(
                                _state.Tableaus[i].ElementAt(j),
                                cardFreed: FreedAction.FREED);
                            a.Execute(State);
                            commands.Enqueue(a);
                        }
                        else break;
                    }
                }
            }
            return commands;
        }
        #endregion

        #region UserInteraction


        public override List<PileKind> GameAction_TrySmartMoveCard(Card card)
        {
            Log($"USER GameAction_TrySmartMoveCard {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            PileKind targetPileKind = default;

            ExecuteAction(() =>
            {
                GameCommand command = default;

                command = TryMoveCardToAnyTableau(card, sourcePileData);
                if (command is { Success: true })
                {
                    targetPileKind = PileKind.TABLEAU;
                }
                else
                {
                    command = TryMoveCardToFreeCell(card, sourcePileData);
                    if (command is { Success: true })
                    {
                        targetPileKind = PileKind.FREECELL;
                    }
                    else
                    {
                        command = TryMoveCardToAnyFoundation(card, sourcePileData);
                        if (command is { Success: true })
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                    }
                }

                if (command is { Success: true })
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    if (sourcePileData.Kind != targetPileKind)
                        affectedPiles.Add(targetPileKind);
                }
                return command;
            });

            return affectedPiles;
        }

        public override List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex)
        {
            if (pileKind != PileKind.STOCK)
                return new List<PileKind>();

            return CommonGameAction_DrawFromStockOrRestock();
        }
        #endregion

        #region AutomaticActions
        public override List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card)
        {
            Log($"INNER MoveCardAutomatically {card}");
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            List<PileKind> affectedPiles = new();
            ExecuteAction(() =>
            {
                GameCommand command = TryMoveCardToAnyFoundation(card, sourcePileData);
                if (command is { Success: true })
                {
                    affectedPiles.Add(PileKind.FOUNDATION);
                    affectedPiles.Add(sourcePileData.Kind);
                }
                return command;
            });
            return affectedPiles;
        }
        #endregion

        #region InnerActions
        //ADD TO ANY
        private GameCommand TryMoveCardToAnyFoundation(Card card, PileData sourcePileData)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");

            GameCommand command = default;
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                command = TryMoveCardToFoundationIndex(card, sourcePileData, i);
                if (command is { Success: true })
                {
                    break;
                }
            }
            return command;
        }

        private GameCommand TryMoveCardToAnyTableau(Card card, PileData sourcePileData)
        {
            int excludeIndex = -99;
            if (sourcePileData.Kind == PileKind.TABLEAU)
            {
                excludeIndex = sourcePileData.Index;
            }

            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            GameCommand command = default;

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
                command = TryMoveCardsToTableauIndex(card, sourcePileData, compatibleFullCandidates[i]);
                if (command is { Success: true })
                {
                    break;
                }
            }

            if (command == null || !command.Success)
            {
                for (int i = 0; i < emptyCandidates.Count; i++)
                {
                    command = TryMoveCardsToTableauIndex(card, sourcePileData, emptyCandidates[i]);
                    if (command is { Success: true })
                    {
                        break;
                    }
                }
            }

            return command;
        }

        //ADD TO INDEX
        private GameCommand TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Log($"INNER TryMoveCardToFoundationIndex {card} > F[{targetPileIndex}]");
            Queue<GameCommandAction> commandActions = new();

            if (CanAddCardToPile(card, PileKind.FOUNDATION, targetPileIndex))
            {
                GameCommandAction a = new GameCommandActionMove(
                    sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                    targetPile: PileKind.FOUNDATION, targetIndex: targetPileIndex
                );
                a.Execute(State);
                commandActions.Enqueue(a);

                a = new GameCommandActionFree(
                    card,
                    cardFreed: FreedAction.LOCKED
                );
                a.Execute(State);
                commandActions.Enqueue(a);

                Queue<GameCommandAction> extraMoves = CommonInner_AfterRemovingCardFromPile(sourcePileData);
                while (extraMoves.Count > 0)
                {
                    commandActions.Enqueue(extraMoves.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        private GameCommand TryMoveCardsToTableauIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Log($"INNER TryMoveCardsToTableauIndex {card} > T[{targetPileIndex}]");
            Queue<GameCommandAction> commandActions = new();

            if (CanAddCardToPile(card, PileKind.TABLEAU, targetPileIndex)) //VALIDATION DONE
            {
                switch (sourcePileData.Kind)
                {
                    case PileKind.TABLEAU:
                        GameCommandAction moveStackAction = new GameCommandActionMoveStack(
                           card,
                           sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                           targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                       );
                        moveStackAction.Execute(State);
                        commandActions.Enqueue(moveStackAction);
                        break;
                    case PileKind.WASTE:
                    case PileKind.FREECELL:
                        GameCommandAction moveAction = new GameCommandActionMove(
                                    sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                                    targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                                );
                        moveAction.Execute(State);
                        commandActions.Enqueue(moveAction);
                        break;
                }

                Queue<GameCommandAction> extraCommands = CommonInner_AfterRemovingCardFromPile(sourcePileData);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        private GameCommand TryMoveCardToFreeCell(Card card, PileData sourcePileData)
        {
            Queue<GameCommandAction> commandActions = new();
            if (CanAddCardToPile(card, PileKind.FREECELL, -1))
            {
                GameCommandAction a = new GameCommandActionMove(
                    sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                    targetPile: PileKind.FREECELL, targetIndex: 0
                );
                a.Execute(State);
                commandActions.Enqueue(a)
;
                Queue<GameCommandAction> extraCommands = CommonInner_AfterRemovingCardFromPile(sourcePileData);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
                return new GameCommand(commandActions);
            }
            return null;
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
        public override Card GetSolvableCard()
        {
            Debug.Log("Looking for automatic move");

            Card card;

            _state.FreeCells[0].TryPeek(out card);
            if (card != null && IsSafeToMoveCardToFoundation(card))
            {
                Debug.Log($"Safe to move {card} to foundation. Can move?");
                if (CanMoveCardToAnyFoundation(card))
                {
                    Debug.Log($"{card} can be moved from stock (free cell mode) to foundation.");
                    return card;
                }
            }

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