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
        private bool _drewAllCardsFromStock;

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
                    if (j > 0)
                    {
                        if (ValidTableauCardStack(_state.Tableaus[i].ElementAt(j - 1), _state.Tableaus[i].ElementAt(j)))
                        {
                            commands.Enqueue(new GameCommandAction(
                                _state.Tableaus[i].ElementAt(j),
                                cardFreed: FreedAction.FREED));
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
                    case PileKind.STOCK:
                        command = TryMoveCardToStock(card, sourcePileData);
                        break;
                }

                if (command != null && command.Actions.Count > 0)
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
                int targetPileIndex = -99;
                command = TryMoveCardToAnyTableau(card, out targetPileIndex, sourcePileData.Index);

                if (command != null && command.Actions.Count > 0)
                {
                    targetPileKind = PileKind.TABLEAU;
                }
                else
                {
                    command = TryMoveCardToStock(card, sourcePileData);
                    if (command != null && command.Actions.Count > 0)
                    {
                        targetPileKind = PileKind.STOCK;
                        targetPileIndex = -1;
                    }
                    else
                    {
                        command = TryMoveCardToAnyFoundation(card, sourcePileData, out targetPileIndex);
                        if (command != null && command.Actions.Count > 0)
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                    }
                }

                if (command != null && command.Actions.Count > 0)
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
                int targetPileIndex = -99;
                GameCommand command = TryMoveCardToAnyFoundation(card, sourcePileData, out targetPileIndex);
                if (command != null && command.Actions.Count > 0)
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
            if (_drewAllCardsFromStock || _state.StockPile.Count == 0)
            {
                return null;
            }

            Queue<GameCommandAction> commands = new();

            if (_state.WastePile.TryPeek(out var topCard))
                commands.Enqueue(new GameCommandAction(
                    topCard,
                    cardFreed: FreedAction.LOCKED
                ));

            for (int i = _state.StockPile.Count - 1; i >= Mathf.Max(0, _state.StockPile.Count - DrawCount); i--)
            {
                commands.Enqueue(new GameCommandAction(
                    _state.StockPile.ElementAt(i),
                    cardMoved: true,
                    sourcePile: PileKind.STOCK,
                    targetPile: PileKind.WASTE,
                    cardRevealed: RevealedAction.REVEALED
               ));
            }

            if (_state.StockPile.Count == 0)
            {
                _drewAllCardsFromStock = true;
                EventManager.OnStockEmpty?.Invoke(true);
            }

            if (_state.WastePile.TryPeek(out topCard) && !topCard.Free)
                commands.Enqueue(new GameCommandAction(
                     topCard,
                     cardFreed: FreedAction.FREED
                 ));

            return new GameCommand(commands);
        }

        //ADD TO ANY
        private GameCommand TryMoveCardToAnyFoundation(Card card, PileData sourcePileData, out int targetPileIndex)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");
            targetPileIndex = -99;
            GameCommand command = default;
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (card.Value > 1 && _state.Foundations[i].Suit != card.Suit)
                    continue;

                command = TryMoveCardToFoundationIndex(card, sourcePileData, i);
                if (command != null && command.Actions.Count > 0)
                {
                    targetPileIndex = i;

                }
            }
            return command;
        }

        //ADD TO INDEX
        private GameCommand TryMoveCardToFoundationIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            Queue<GameCommandAction> commandActions = new();
            if (CanAddCardToPile(card, PileKind.FOUNDATION, targetPileIndex))
            {
                commandActions.Enqueue(new GameCommandAction(
                    card,
                    cardFreed: FreedAction.LOCKED,
                    cardMoved: true,
                    sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                    targetPile: PileKind.FOUNDATION, targetIndex: targetPileIndex
                ));
                Queue<GameCommandAction> extraMoves = AfterRemovingCardFromPile(card);
                while (extraMoves.Count > 0)
                {
                    commandActions.Enqueue(extraMoves.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        private GameCommand TryMoveCardToAnyTableau(Card card, out int targetPileIndex, int excludeIndex = -1)
        {
            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            PileData soucePileData = _state.GetCardPileOwnerData(card);
            GameCommand command = default;
            targetPileIndex = -99;

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
                command = TryMoveCardsToTableauIndex(card, soucePileData, compatibleFullCandidates[i]);
                if (command != null && command.Actions.Count > 0)
                {
                    targetPileIndex = i;
                    break;
                }
            }

            if (command == null || command.Actions.Count == 0)
            {
                for (int i = 0; i < emptyCandidates.Count; i++)
                {
                    command = TryMoveCardsToTableauIndex(card, soucePileData, emptyCandidates[i]);
                    if (command != null && command.Actions.Count > 0)
                    {
                        targetPileIndex = i;
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
                Queue<GameCommandAction> extraCommands = new();
                switch (sourcePileData.Kind)
                {
                    case PileKind.TABLEAU:
                        Stack<GameCommandAction> cardsInTableauToMove = new();
                        Stack<Card> fromTableau = _state.Tableaus[sourcePileData.Index];

                        for (int i = fromTableau.Count - 1; i >= 0; i--)
                        {
                            bool cardMatch = fromTableau.ElementAt(i) != card;
                            cardsInTableauToMove.Push(new GameCommandAction(
                                fromTableau.ElementAt(i),
                                cardMoved:true,
                                sourcePile: PileKind.TABLEAU, sourceIndex: sourcePileData.Index,
                                targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                            ));
                            if (cardMatch)
                            {
                                while (cardsInTableauToMove.Count > 0)
                                    commandActions.Enqueue(cardsInTableauToMove.Pop());
                                if (i > 0)
                                {
                                    if (!fromTableau.ElementAt(i - 1).Free || !fromTableau.ElementAt(i - 1).Revealed)
                                    {
                                        commandActions.Enqueue(new GameCommandAction(
                                            fromTableau.ElementAt(i - 1),
                                            cardRevealed: fromTableau.ElementAt(i - 1).Revealed ? RevealedAction.NO_CHANGE : RevealedAction.REVEALED,
                                            cardFreed: fromTableau.ElementAt(i - 1).Free ? FreedAction.NO_CHANGE : FreedAction.FREED
                                        ));
                                    }
                                }
                                break;
                            }
                        }
                        extraCommands = UpdateFreeCards();
                        break;
                    case PileKind.WASTE:
                        commandActions.Enqueue(new GameCommandAction(
                            card,
                            sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                            targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex,
                            cardMoved: true
                        ));
                        extraCommands = AfterRemovingCardFromPile(card);
                        break;
                    case PileKind.STOCK:
                        commandActions.Enqueue(new GameCommandAction(
                            card,
                            cardMoved: true,
                            sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                            targetPile: PileKind.TABLEAU, targetIndex: targetPileIndex
                        ));
                        extraCommands = AfterRemovingCardFromPile(card);
                        break;
                }
                while (extraCommands.Count > 0)
                {
                    commandActions.Enqueue(extraCommands.Dequeue());
                }
            }
            return new GameCommand(commandActions);
        }

        private GameCommand TryMoveCardToStock(Card card, PileData sourcePileData)
        {
            if (CanAddCardToPile(card, PileKind.STOCK, -1))
            {
                return new GameCommand(new GameCommandAction(
                    card,
                    cardMoved: true,
                    sourcePile: sourcePileData.Kind, sourceIndex: sourcePileData.Index,
                    targetPile: PileKind.STOCK
                ));
            }
            return null;
        }

        // REMOVE
        private Queue<GameCommandAction> AfterRemovingCardFromPile(Card card)
        {
            Log($"INNER AfterRemovingCardFromPile {card}");
            Queue<GameCommandAction> result = new();
            PileData pileData = _state.GetCardPileOwnerData(card);
            switch (pileData.Kind)
            {
                case PileKind.WASTE:
                    // _state.WastePile.Pop();
                    if (_state.WastePile.Count > 0)
                    {
                        result.Enqueue(new GameCommandAction(
                            _state.WastePile.Peek(),
                            cardFreed: FreedAction.FREED)
                        );
                    }
                    break;
                case PileKind.TABLEAU:
                    if (_state.Tableaus[pileData.Index].Peek() == card)
                    {
                        // _state.Tableaus[pileData.Index].Pop();
                        //if (_state.Tableaus[pileData.Index].Count > 0)
                        if (_state.Tableaus[pileData.Index].Count > 1)
                        {
                            Card tCard = _state.Tableaus[pileData.Index].Peek();
                            result.Enqueue(new GameCommandAction(
                                tCard,
                                cardRevealed: tCard.Revealed ? RevealedAction.NO_CHANGE : RevealedAction.REVEALED,
                                cardFreed: tCard.Free ? FreedAction.NO_CHANGE : FreedAction.FREED));
                            Queue<GameCommandAction> extraCommands = UpdateFreeCards();
                            while (extraCommands.Count > 0)
                                result.Enqueue(extraCommands.Dequeue());
                        }
                    }
                    break;
                case PileKind.STOCK:
                    //_state.StockPile.Pop();
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

        public override Card GetSolvableCard()
        {
            Debug.Log("Looking for automatic move");

            Card card;

            if (_drewAllCardsFromStock && _state.StockPile.TryPeek(out card))
            {
                if (card != null && IsSafeToMoveCardToFoundation(card))
                {
                    Debug.Log($"Safe to move {card} to foundation. Can move?");
                    if (CanMoveCardToAnyFoundation(card))
                    {
                        Debug.Log($"{card} can be moved from stock (free cell mode) to foundation.");
                        return card;
                    }
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