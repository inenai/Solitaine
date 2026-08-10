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
        public int DrawCount => 3;

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
            _state = new SawayamaState(
                foundations: 4,
                tableaus: 7,
                freeCells: 1,
                stock: true,
                waste: true
            );
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
        public override List<PileKind> GameAction_TryMoveCardToPile(Card card, PileKind targetPileKind, int targetPileIndex)
        {
            Log($"USER Action_DragCardToPile {card} > {targetPileKind}[{targetPileIndex}]");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            ExecuteAction(() =>
            {
                GameCommand command = default;
                switch (targetPileKind)
                {
                    case PileKind.FOUNDATION:
                        command = TryMoveCardToFoundationIndex(card, sourcePileData, targetPileIndex);
                        break;
                    case PileKind.TABLEAU:
                        command = TryMoveCardsToTableauIndex(card, sourcePileData, targetPileIndex);
                        break;
                    case PileKind.FREECELL:
                        command = TryMoveCardToFreeCell(card, sourcePileData);
                        break;
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

            return Action_TryDrawCardsFromStock();
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
        // DEAL
        private GameCommand TryDrawCardsFromStock()
        {
            Log("INNER TryDrawCardsFromStock");
            if (_state.StockPile.Count == 0)
            {
                return null;
            }

            Queue<GameCommandAction> commands = new();

            if (_state.WastePile.TryPeek(out var topCard))
            {
                GameCommandAction a = new GameCommandActionFree(
                    topCard,
                    cardFreed: FreedAction.LOCKED
                );
                a.Execute(State);
                commands.Enqueue(a);
            }

            int stockFinalAmount = Mathf.Max(0, _state.StockPile.Count - DrawCount);
            for (int i = _state.StockPile.Count - 1; i >= stockFinalAmount; i--)
            {
                GameCommandAction a = new GameCommandActionMove(
                        sourcePile: PileKind.STOCK,
                        targetPile: PileKind.WASTE
                   );
                a.Execute(State);
                commands.Enqueue(a);

                a = new GameCommandActionReveal(
                     _state.WastePile.Peek(),
                    cardRevealed: RevealedAction.REVEALED
                );
                a.Execute(State);
                commands.Enqueue(a);

                if (i == stockFinalAmount)
                {
                    a = new GameCommandActionFree(
                       _state.WastePile.Peek(),
                      cardFreed: FreedAction.FREED
                   );
                    a.Execute(State);
                    commands.Enqueue(a);
                }
            }
            return new GameCommand(commands);
        }

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

                Queue<GameCommandAction> extraCommands = AfterRemovingCardFromPile(sourcePileData);
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
                Queue<GameCommandAction> extraCommands = AfterRemovingCardFromPile(sourcePileData);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
                return new GameCommand(commandActions);
            }
            return null;
        }

        // REMOVE
        private Queue<GameCommandAction> AfterRemovingCardFromPile(PileData pileData)
        {
            Log($"INNER RemovedCardFromPile {pileData.Kind}[{pileData.Index}]");
            Queue<GameCommandAction> result = new();

            switch (pileData.Kind)
            {
                case PileKind.WASTE:
                    if (_state.WastePile.Count > 0)
                    {
                        GameCommandAction a = new GameCommandActionFree(
                            _state.WastePile.Peek(),
                            cardFreed: FreedAction.FREED
                        );
                        a.Execute(State);
                        result.Enqueue(a);
                    }
                    break;
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