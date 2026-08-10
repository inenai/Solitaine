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

        FreeCellGameState _state;

        public FreeCellGameGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            ShuffleAndDealDeck(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new FreeCellGameState(
              foundations: 4,
              tableaus: 8,
              freeCells: 4,
              stock: false,
              waste: false
          );
        }

        #region Initialization

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
                    command = TryMoveCardToAnyFreeCell(card, sourcePileData);
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
                    return command;
                }

                return null;
            });

            return affectedPiles;
        }

        public override List<PileKind> GameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Log($"USER Action_DragCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            ExecuteAction(() =>
            {
                GameCommand command = default; ;
                switch (targetPileKind)
                {
                    case PileKind.FOUNDATION:
                        command = TryMoveCardToFoundationIndex(card, sourcePileData, targetPileIndex);
                        break;
                    case PileKind.TABLEAU:
                        command = TryMoveCardsToTableauIndex(card, sourcePileData, targetPileIndex);
                        break;
                    case PileKind.FREECELL:
                        command = TryMoveCardToFreeCellIndex(card, sourcePileData, targetPileIndex);
                        break;
                }

                if (command is { Success: true })
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    if (sourcePileData.Kind != targetPileKind)
                        affectedPiles.Add(targetPileKind);

                    return command;
                }

                return null;
            });

            return affectedPiles;
        }

        public override List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex)
        {
            return new List<PileKind>();
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

        private GameCommand TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Foundation foundation = _state.Foundations[targetPileIndex];
            Log($"INNER TryMoveCardToFoundationIndex{card} > {foundation}]");
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

                Queue<GameCommandAction> extraCommands = AfterRemovingCardFromPile(sourcePileData);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        //REMOVE
        private Queue<GameCommandAction> AfterRemovingCardFromPile(PileData pileData)
        {
            Log($"INNER RemovedCardFromPile {pileData.Kind}[{pileData.Index}]");
            Queue<GameCommandAction> result = new();

            switch (pileData.Kind)
            {
                case PileKind.TABLEAU:
                    if (_state.Tableaus[pileData.Index].Count > 0)
                    {
                        Card tCard = _state.Tableaus[pileData.Index].Peek();
                        if (!tCard.Revealed)
                        {
                            GameCommandAction a = new GameCommandActionReveal(
                                tCard,
                                cardRevealed: RevealedAction.REVEALED);
                            a.Execute(State);
                            result.Enqueue(a);
                        }
                        if (!tCard.Free)
                        {
                            GameCommandAction a = new GameCommandActionFree(
                                tCard,
                                cardFreed: FreedAction.FREED);
                            a.Execute(State);
                            result.Enqueue(a);
                        }
                    }
                    Queue<GameCommandAction> extraCommands = UpdateFreeCards();
                    while (extraCommands.Count > 0)
                    {
                        result.Enqueue(extraCommands.Dequeue());
                    }
                    break;
            }
            return result;
        }

        private Queue<GameCommandAction> UpdateFreeCards()
        {
            Queue<GameCommandAction> commands = new();
            for (int i = 0; i < 8; i++)
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

        private GameCommand TryMoveCardToAnyFreeCell(Card card, PileData sourcePileData)
        {
            Log($"INNER TryMoveCardToAnyFreeCell {card} > FC*");
            GameCommand command = default;
            for (int i = 0; i < _state.FreeCells.Length; i++)
            {
                if (_state.FreeCells[i].Count > 0)
                    continue;

                command = TryMoveCardToFreeCellIndex(card, sourcePileData, i);
                if (command is { Success: true })
                {
                    break;
                }
            }
            return command;
        }

        private GameCommand TryMoveCardToFreeCellIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Log($"INNER TryMoveCardToFreeCellIndex{card} > FC[{targetPileIndex}]");
            Queue<GameCommandAction> commandActions = new();
            if (CanAddCardToPile(card, PileKind.FREECELL, targetPileIndex))
            {
                GameCommandAction a = new GameCommandActionMove(
                     sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                     targetPile: PileKind.FREECELL, targetIndex: targetPileIndex
                 );
                a.Execute(State);
                commandActions.Enqueue(a);

                Queue<GameCommandAction> extraMoves = AfterRemovingCardFromPile(sourcePileData);
                while (extraMoves.Count > 0)
                {
                    a = extraMoves.Dequeue();
                    a.Execute(State);
                    commandActions.Enqueue(a);
                }
            }
            return new GameCommand(commandActions);
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
                    case PileKind.FREECELL:
                        GameCommandAction moveAction = new GameCommandActionMove(
                            sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                            targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                        );
                        moveAction.Execute(State);
                        commandActions.Enqueue(moveAction);
                        break;
                }
                Queue<GameCommandAction> extraCommands = AfterRemovingCardFromPile(sourcePileData);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }
        #endregion

        #region Checks

        protected override bool Won()
        {
            int total = 0;
            foreach (Foundation f in _state.Foundations)
            {
                total += f.Stack.Count;
            }
            return total == 13 * 4;
        }

        private bool ValidTableauCardStack(Card child, Card parent)
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

        public override Card GetSolvableCard()
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

        #endregion


    }
}