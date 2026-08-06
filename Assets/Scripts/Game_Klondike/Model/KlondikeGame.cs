using System.Collections.Generic;
using Common;
using static Utils.CommonUtils;
using Utils;
using UnityEngine;
using System.Linq;

namespace Klondike
{
    public class KlondikeGame : Game
    {
        public override GameState State => _state;

        public KlondikeState KState => _state;
        protected override string DebugTag => "Klondike";

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
            _state = new KlondikeState(
                foundations: 4,
                tableaus: 7,
                freeCells: 0,
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

        protected override bool Won()
        {
            int total = 0;
            foreach (Foundation f in _state.Foundations)
            {
                total += f.Stack.Count;
            }
            return total == 13 * 4;
        }
        #endregion

        #region userInteraction


        /// <summary>
        /// </summary>
        /// <param name="card"></param>
        /// <returns><para>A list of the pile kinds that have been affected and should be updated in the ui.</para>
        /// <para>If returned list is not empty you must call UIDoneRefreshing when UI has finished updating. </para></returns>
        public override List<PileKind> GameAction_TrySmartMoveCard(Card card)
        {
            Log($"USER GameAction_TrySmartMoveCard {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);
            PileKind targetPileKind = default;

            ExecuteAction(() =>
            {
                GameCommand command = default;
                switch (sourcePileData.Kind)
                {
                    case PileKind.WASTE:
                        if (IsSafeToMoveCardToFoundation(card))
                        {
                            command = TryMoveCardToAnyFoundation(card, sourcePileData);
                        }

                        if (command != null && command.Actions.Count > 0)
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                        else
                        {
                            command = TryMoveCardToAnyTableau(card);
                            if (command != null && command.Actions.Count > 0)
                            {
                                targetPileKind = PileKind.TABLEAU;
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
                        break;
                    case PileKind.FOUNDATION:
                        command = TryMoveCardToAnyTableau(card);
                        if (command is { Success: true })
                            targetPileKind = PileKind.TABLEAU;
                        break;
                    case PileKind.TABLEAU:
                        command = TryMoveCardToAnyFoundation(card, sourcePileData);
                        if (command is { Success: true })
                            targetPileKind = PileKind.FOUNDATION;
                        else
                        {
                            command = TryMoveCardToAnyTableau(card, sourcePileData.Index);
                            if (command is { Success: true })
                                targetPileKind = PileKind.TABLEAU;
                        }
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

        public override List<PileKind> GameAction_ClickedPile(PileKind pileKind, int pileIndex)
        {
            if (pileKind != PileKind.STOCK)
                return new List<PileKind>();

            List<PileKind> affectedPiles = new List<PileKind>();
            bool uiRefreshNeeded = ExecuteAction(() =>
            {
                GameCommand command = default;
                int drewAmount = 0;
                command = TryDrawCardsFromStock(out drewAmount);
                if (command is { Success: true })
                {
                    return command;
                }
                else
                {
                    command = TryRestock();
                }
                return command;
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
        #endregion

        #region AutomaticActions
        public override List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card)
        {
            Log($"USER GameAction_TryMoveCardToFoundationAutomatic {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            if (sourcePileData.Kind == PileKind.FOUNDATION || sourcePileData.Kind == PileKind.STOCK)
                return new List<PileKind>();

            ExecuteAction(() =>
            {
                GameCommand command = TryMoveCardToAnyFoundation(card, sourcePileData);

                if (command != null && command.Actions.Count > 0)
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    affectedPiles.Add(PileKind.FOUNDATION);
                }
                return command;
            });

            return affectedPiles;
        }
        #endregion

        #region innerActions

        // DEAL
        private GameCommand TryDrawCardsFromStock(out int drewAmount)
        {
            Log("INNER TryDrawCardsFromStock");
            drewAmount = 0;

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

            int stockFinalAmount = Mathf.Max(0, _state.StockPile.Count - _state.DrawCount);
            for (int i = _state.StockPile.Count - 1; i >= stockFinalAmount; i--)
            {
                GameCommandAction a = new GameCommandActionMove(
                       sourcePile: PileKind.STOCK,
                       targetPile: PileKind.WASTE
                  );
                a.Execute(State);
                commands.Enqueue(a);

                a = new GameCommandActionReveal(
                    _state.WastePile.ElementAt(i),
                    cardRevealed: RevealedAction.REVEALED
                );
                a.Execute(State);
                commands.Enqueue(a);

                if (i == stockFinalAmount)
                {
                    a = new GameCommandActionFree(
                      _state.WastePile.ElementAt(i),
                      cardFreed: FreedAction.FREED
                   );
                    a.Execute(State);
                    commands.Enqueue(a);
                }
            }
            return new GameCommand(commands);
        }

        private GameCommand TryRestock()
        {
            Log("INNER AttemptRestock");

            Queue<GameCommandAction> commands = new();
            if (_state.WastePile.Count > 0 && _state.AvailableRestocks != 0)
            {
                GameCommandAction a = new GameCommandActionRestock();
                a.Execute(State);
                commands.Enqueue(a);
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
            }

            return command;
        }

        private GameCommand TryMoveCardToAnyTableau(Card card, int excludeIndex = -1)
        {
            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            PileData soucePileData = _state.GetCardPileOwnerData(card);
            GameCommand command = default;

            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                if (i == excludeIndex) continue;

                command = TryMoveCardsToTableauIndex(card, soucePileData, i);
                if (command is { Success: true })
                {
                    break;
                }
            }
            return command;
        }

        //ADD TO INDEX
        private GameCommand TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Queue<GameCommandAction> commandActions = new();
            Foundation foundation = _state.Foundations[targetPileIndex];
            Log($"INNER TryMoveCardToFoundationIndex{card} > {foundation}]");
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
                        Stack<GameCommandAction> cardsInTableauToMove = new();
                        Stack<Card> fromTableau = _state.Tableaus[sourcePileData.Index];

                        for (int i = fromTableau.Count - 1; i >= 0; i--)
                        {
                            bool cardMatch = fromTableau.ElementAt(i) != card;
                            cardsInTableauToMove.Push(new GameCommandActionMove(
                                sourcePile: PileKind.TABLEAU, sourceIndex: sourcePileData.Index,
                                targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                            ));
                            if (cardMatch)
                            {
                                while (cardsInTableauToMove.Count > 0)
                                {
                                    GameCommandAction moveCardAction = cardsInTableauToMove.Pop();
                                    moveCardAction.Execute(State);
                                    commandActions.Enqueue(moveCardAction);
                                }
                                break;
                            }
                        }
                        break;
                    case PileKind.FOUNDATION:
                    case PileKind.WASTE:
                        GameCommandAction moveAction = new GameCommandActionMove(
                                   sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                                   targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                               );
                        moveAction.Execute(State);
                        commandActions.Enqueue(moveAction);
                        break;
                }
                Queue<GameCommandAction> extraCommands = AfterRemovingCardFromPile(card);
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        // REMOVE
        private Queue<GameCommandAction> AfterRemovingCardFromPile(Card card)
        {
            Log($"INNER RemoveCardFromPile {card}");
            Queue<GameCommandAction> result = new();
            PileData pileData = _state.GetCardPileOwnerData(card);
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
                        if (_state.Tableaus[pileData.Index].Count > 1)
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
                    }
                    break;
            }
            return result;
        }
        #endregion

        #region Checks

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
        private bool ValidTableauCardStack(Card child, Card parent)
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