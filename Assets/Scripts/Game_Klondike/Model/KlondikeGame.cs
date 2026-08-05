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
            _state.OnRestocked();
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
                int targetPileIndex = -99;
                bool cardMoved = false;

                switch (sourcePileData.Kind)
                {
                    case PileKind.WASTE:
                        if (IsSafeToMoveCardToFoundation(card))
                        {
                            cardMoved = TryMoveCardToAnyFoundation(card, out targetPileIndex);
                        }

                        if (cardMoved)
                        {
                            targetPileKind = PileKind.FOUNDATION;
                        }
                        else
                        {
                            cardMoved = TryMoveCardToAnyTableau(card, out targetPileIndex);
                            if (cardMoved)
                            {
                                targetPileKind = PileKind.TABLEAU;
                            }
                            else
                            {
                                cardMoved = TryMoveCardToAnyFoundation(card, out targetPileIndex);
                                if (cardMoved)
                                {
                                    targetPileKind = PileKind.FOUNDATION;
                                }
                            }
                        }
                        break;
                    case PileKind.FOUNDATION:
                        cardMoved = TryMoveCardToAnyTableau(card, out targetPileIndex);
                        if (cardMoved) targetPileKind = PileKind.TABLEAU;
                        break;
                    case PileKind.TABLEAU:
                        cardMoved = TryMoveCardToAnyFoundation(card, out targetPileIndex);
                        if (cardMoved) targetPileKind = PileKind.FOUNDATION;
                        if (!cardMoved)
                        {
                            cardMoved = TryMoveCardToAnyTableau(card, out targetPileIndex, sourcePileData.Index);
                            if (cardMoved) targetPileKind = PileKind.TABLEAU;
                        }
                        break;
                }

                if (cardMoved)
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    if (sourcePileData.Kind != targetPileKind)
                        affectedPiles.Add(targetPileKind);

                    return new GameCommand(sourcePileData.Kind, sourcePileData.Index, targetPileKind, targetPileIndex, false, false);
                }
                return null;
            });

            return affectedPiles;
        }

        public override List<PileKind> AutoAction_TryMoveCardToFoundationAutomatically(Card card)
        {
            Log($"USER GameAction_TryMoveCardToFoundationAutomatic {card}");
            List<PileKind> affectedPiles = new List<PileKind>();
            PileData sourcePileData = _state.GetCardPileOwnerData(card);

            if (sourcePileData.Kind == PileKind.FOUNDATION || sourcePileData.Kind == PileKind.STOCK)
                return new List<PileKind>();

            ExecuteAction(() =>
            {
                int targetPileIndex = -99;
                bool cardMoved = TryMoveCardToAnyFoundation(card, out targetPileIndex);

                if (cardMoved)
                {
                    affectedPiles.Add(sourcePileData.Kind);
                    affectedPiles.Add(PileKind.FOUNDATION);

                    return new GameCommand(sourcePileData.Kind, sourcePileData.Index, PileKind.FOUNDATION, targetPileIndex, true);
                }
                return null;
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
                int drewAmount = 0;
                if (TryDrawCardsFromStock(out drewAmount))
                {
                    return new GameCommand(PileKind.STOCK, -1, PileKind.WASTE, -1, false, drewAmount);
                }
                else
                {
                    if (TryRestock())
                    {
                        return new GameCommand(PileKind.WASTE, -1, PileKind.STOCK, -1, false, _state.StockPile.Count);
                    }
                }
                return null;
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
                bool cardMoved = false;
                switch (targetPileKind)
                {
                    case PileKind.FOUNDATION:
                        cardMoved = TryMoveCardToFoundationIndex(card, targetPileIndex);
                        break;
                    case PileKind.TABLEAU:
                        cardMoved = TryMoveCardsToTableauIndex(card, targetPileIndex);
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
        #endregion


        #region innerActions

        // DEAL
        private bool TryDrawCardsFromStock(out int drewAmount)
        {
            Log("INNER TryDrawCardsFromStock");
            drewAmount = 0;

            if (_state.StockPile.Count == 0)
            {
                return false;
            }

            if (_state.WastePile.TryPeek(out var topCard))
                topCard.FreeCard(false);

            for (int i = 0; i < _state.DrawCount; i++)
            {
                if (_state.StockPile.Count > 0)
                {
                    Card nextCard = _state.StockPile.Pop();
                    nextCard.Show(true);
                    _state.WastePile.Push(nextCard);
                }
                else break;
            }

            if (_state.WastePile.TryPeek(out topCard))
                topCard.FreeCard(true);

            return true;
        }

        private bool TryRestock()
        {
            Log("INNER AttemptRestock");
            if (_state.WastePile.Count > 0 && _state.AvailableRestocks != 0)
            {
                while (_state.WastePile.Count > 0)
                {
                    Card nextCard = _state.WastePile.Pop();
                    nextCard.Show(false);
                    _state.StockPile.Push(nextCard);
                }
                _state.OnRestocked();
                return true;
            }
            return false;
        }

        //ADD TO ANY
        private bool TryMoveCardToAnyFoundation(Card card, out int targetPileIndex)
        {
            Log($"INNER TryMoveCardToAnyFoundation {card} > F*");
            for (int i = 0; i < _state.Foundations.Length; i++)
            {
                if (TryMoveCardToFoundationIndex(card, i))
                {
                    targetPileIndex = i;
                    return true;
                }
            }
            targetPileIndex = -99;
            return false;
        }

        private bool TryMoveCardToAnyTableau(Card card, out int targetPileIndex, int excludeIndex = -1)
        {
            Log($"INNER TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            bool success = false;
            targetPileIndex = -99;
            for (int i = 0; i < _state.Tableaus.Length; i++)
            {
                if (i == excludeIndex) continue;

                success = TryMoveCardsToTableauIndex(card, i);
                if (success)
                {
                    targetPileIndex = i;
                    break;
                }
            }
            return success;
        }

        //ADD TO INDEX
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
                card.FreeCard(_state.FoundationCardsFree);
                return true;
            }
            return false;
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
                        }
                        while (tempStack.Count > 0)
                        {
                            _state.Tableaus[index].Push(tempStack.Pop());
                        }
                        break;
                    case PileKind.FOUNDATION:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                    case PileKind.WASTE:
                        RemoveCardFromPile(card);
                        _state.Tableaus[index].Push(card);
                        break;
                }
                return true;
            }
            return false;
        }

        // REMOVE
        private void RemoveCardFromPile(Card card)
        {
            Log($"INNER RemoveCardFromPile {card}");
            PileData pileData = _state.GetCardPileOwnerData(card);
            switch (pileData.Kind)
            {
                case PileKind.WASTE:
                    _state.WastePile.Pop();
                    if (_state.WastePile.Count > 0)
                    {
                        _state.WastePile.Peek().FreeCard(true);
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
                        _state.Tableaus[pileData.Index].Pop();
                        if (_state.Tableaus[pileData.Index].Count > 0)
                        {
                            _state.Tableaus[pileData.Index].Peek().FreeCard(true);
                            _state.Tableaus[pileData.Index].Peek().Show(true);
                        }
                    }
                    break;
            }
        }

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
        #endregion

        #region Checks
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

    }
}