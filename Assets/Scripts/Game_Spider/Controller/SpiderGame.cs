using System.Collections.Generic;
using static Utils.CommonUtils;
using Common;
using UnityEngine;

namespace Spider
{
    public class SpiderGame : Game
    {
        public override GameState State => _state;
        public SpiderState SState => _state;

        public override int FoundationsAmount => 8;

        public override int TableausAmount => 10;

        public override int FreeCellsAmount => 0;

        public override bool HasStock => true;

        public override bool HasWaste => false;

        protected override string DebugTag => "Spider";

        protected override int DrawCount => TableausAmount;

        SpiderState _state;

        public SpiderGame(List<Card> deck)
        {
            Log("Starting a Klondike game.");
            CreateState();
            ShuffleAndDeal(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new SpiderState(this);
        }

        public override bool CanAddCardToPile(Card card, PileKind targetPile, int targetPileIndex)
        {
            PileData sourcePileData = State.GetCardPileOwnerData(card);
            if (sourcePileData.Kind == targetPile && sourcePileData.Index == targetPileIndex)
                return false;

            switch (targetPile)
            {
                case PileKind.TABLEAU:
                    bool toEmptyTableau = _state.Tableaus[targetPileIndex].Count == 0;
                    bool validStack = !toEmptyTableau && ValidTableauCardStackToPlace(card, _state.Tableaus[targetPileIndex].Peek());

                    return toEmptyTableau || validStack;
                case PileKind.FOUNDATION:
                    bool toEmptyFoundation = _state.Foundations[targetPileIndex].Count == 0;
                    return toEmptyFoundation;
            }
            return false;
        }

        public static List<Card> CreateGameDeck()
        {
            List<Card> deck = new();
            AddOneSetOfCardsToDeck(deck);
            AddOneSetOfCardsToDeck(deck);
            return deck;
        }

        private static void AddOneSetOfCardsToDeck(List<Card> deck)
        {
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(CardSuit.SPADES, i));
            }
            CardSuit suit = SpiderSettings.SuitsAmount > 1 ? CardSuit.HEARTS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
            suit = SpiderSettings.SuitsAmount > 2 ? CardSuit.DIAMONDS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
            suit = SpiderSettings.SuitsAmount > 2 ? CardSuit.CLUBS : CardSuit.SPADES;
            for (int i = 1; i <= 13; i++)
            {
                deck.Add(new Card(suit, i));
            }
        }

        private void ShuffleAndDeal(List<Card> deck)
        {
            Log("Shuffling and dealing...");
            CardPile deckStack = new CardPile(Shuffle(deck.ToArray()));
            int cardsInTabs = 54;

            for (int i = 0; i < cardsInTabs; i++)
            {
                Card nextCard = deckStack.Pop();
                if (i >= (cardsInTabs - TableausAmount))
                {
                    nextCard.Show(true);
                    nextCard.FreeCard(true);
                }
                _state.Tableaus[i % TableausAmount].Push(nextCard);
            }

            _state.StockPile = deckStack;
        }

        protected override bool ValidTableauCardStackToPlace(Card child, Card parent)
        {
            if (child == null || parent == null) return false;
            return child.Value == parent.Value - 1;
        }

        protected override bool ValidTableauCardStackToMoveAround(Card child, Card parent)
        {
            if (child == null || parent == null) return false;
            return child.Value == parent.Value - 1 && child.Suit == parent.Suit;
        }

        public override Card GetSolvableCard()
        {
            Debug.Log("Spider: Looking for automatic move");

            for (int i = 0; i < TableausAmount; i++)
            {
                if (State.Tableaus[i].Count < 13) continue;

                Card validCard = null;
                int lastValue = 0;
                for (int j = 0; j < State.Tableaus[i].Count; j++)
                {
                    if (State.Tableaus[i].ElementAt(j).Value == 13)
                    {
                        validCard = State.Tableaus[i].ElementAt(j);
                        lastValue = 13;
                        continue;
                    }

                    if (validCard == null) continue;

                    if (State.Tableaus[i].ElementAt(j).Suit == validCard.Suit
                    && State.Tableaus[i].ElementAt(j).Value == lastValue - 1)
                    {
                        lastValue = State.Tableaus[i].ElementAt(j).Value;
                    }
                    else
                    {
                        validCard = null;
                        lastValue = 0;
                    }
                }

                if (lastValue == 1)
                {
                    return validCard;
                }
            }

            return null;
        }

        public override List<PileKind> GameAction_TrySmartMoveCard(Card card, PileData sourcePileData)
        {
            Log($"USER GameAction_TrySmartMoveCard {card}");
            List<PileKind> affectedPiles = new List<PileKind>();

            ExecuteAction(() =>
            {
                GameCommand command = default;

                command = CommonInner_TryMoveCardToAnyTableau(card, sourcePileData);

                if (command is { Valid: true })
                {
                    affectedPiles.Add(PileKind.TABLEAU);
                }

                return command;
            });

            return affectedPiles;
        }

        protected override List<PileKind> CommonGameAction_DrawFromStockOrRestock()
        {
            List<PileKind> affectedPiles = new List<PileKind>();
            bool uiRefreshNeeded = ExecuteAction(() =>
            {
                GameCommand command = default;

                command = CommonInner_TryDrawCardsFromStock(out int drewAmount);
                if (command is { Valid: true })
                {
                    return command;
                }
                return command;
            });

            if (uiRefreshNeeded)
            {
                affectedPiles.Add(PileKind.TABLEAU);
                affectedPiles.Add(PileKind.STOCK);
            }
            return affectedPiles;
        }

        protected override GameCommand CommonInner_TryDrawCardsFromStock(out int drewAmount)
        {
            Log("INNER TryDrawCardsFromStock");
            drewAmount = DrawCount;

            if (State.StockPile.Count == 0)
                return null;

            bool emptyTableau = false;
            foreach (CardPile tableau in State.Tableaus)
            {
                if (tableau.Count == 0)
                {
                    emptyTableau = true;
                    break;
                }
            }

            if (emptyTableau)
                return null;

            List<GameCommandAction> commands = new();
            GameCommandAction gca = null;

            for (int i = 0; i < TableausAmount; i++)
            {
                foreach (Card card in State.Tableaus[i])
                {
                    if (card.Free)
                    {
                        gca = new GameCommandActionFree(
                            card,
                            cardFreed: FreedAction.LOCKED
                        );
                        gca.Execute(State);
                        commands.Add(gca);
                    }
                }

                gca = new GameCommandActionMove(
                       sourcePile: PileKind.STOCK,
                       targetPile: PileKind.TABLEAU, targetIndex: i
                  );
                gca.Execute(State);
                commands.Add(gca);

                gca = new GameCommandActionReveal(
                    State.Tableaus[i].Peek(),
                    cardRevealed: RevealedAction.REVEALED
                );
                gca.Execute(State);
                commands.Add(gca);

                gca = new GameCommandActionFree(
                  State.Tableaus[i].Peek(),
                  cardFreed: FreedAction.FREED
                );

                gca.Execute(State);
                commands.Add(gca);
            }
            commands.AddRange(CommonInner_UpdateFreeCards());

            return new GameCommand(commands);
        }

        protected override GameCommand CommonInner_TryMoveCardToAnyTableau(Card card, PileData sourcePileData)
        {
            int excludeIndex = -99;
            if (sourcePileData.Kind == PileKind.TABLEAU)
            {
                excludeIndex = sourcePileData.Index;
                Log($"INNER CommonInner_TryMoveCardToAnyTableau {card} (except to T[{excludeIndex}])");
            }
            else
            {
                Log($"INNER CommonInner_TryMoveCardToAnyTableau {card} > T*");
            }

            GameCommand command = default;

            List<int> emptyCandidates = new();
            List<int> sameSuitCandidates = new();
            List<int> compatibleFullCandidates = new();

            for (int i = 0; i < State.Tableaus.Length; i++)
            {
                if (i == excludeIndex) continue;

                if (CanAddCardToPile(card, PileKind.TABLEAU, i))
                {
                    if (State.Tableaus[i].Count > 0)
                    {
                        if (State.Tableaus[i].Peek().Suit == card.Suit)
                        {
                            sameSuitCandidates.Add(i);
                        }
                        else
                        {
                            compatibleFullCandidates.Add(i);
                        }
                    }
                    else
                    {
                        emptyCandidates.Add(i);
                    }
                }
            }
            int targetTableau = -1;
            for (int i = 0; i < sameSuitCandidates.Count; i++)
            {
                command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, sameSuitCandidates[i]);
                if (command is { Valid: true })
                {
                    targetTableau = sameSuitCandidates[i];
                    break;
                }
            }

            if (command == null || !command.Valid)
            {
                for (int i = 0; i < compatibleFullCandidates.Count; i++)
                {
                    command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, compatibleFullCandidates[i]);
                    if (command is { Valid: true })
                    {
                        targetTableau = compatibleFullCandidates[i];
                        break;
                    }
                }

                if (command == null || !command.Valid)
                {
                    for (int i = 0; i < emptyCandidates.Count; i++)
                    {
                        command = CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, emptyCandidates[i]);
                        if (command is { Valid: true })
                        {
                            break;
                        }
                    }
                }
            }
            return command;
        }

        protected override GameCommand CommonInner_TryMoveCardsToTableauIndex(Card card, PileData sourcePileData, int targetPileIndex)
        {
            GameCommand command = base.CommonInner_TryMoveCardsToTableauIndex(card, sourcePileData, targetPileIndex);

            if (command is { Valid: true } && State.Tableaus[targetPileIndex].Count > 1)
            {
                foreach (Card tcard in State.Tableaus[targetPileIndex])
                {
                    if (tcard != card && tcard.Free)
                    {
                        GameCommandAction gca = new GameCommandActionFree(
                            tcard,
                            cardFreed: FreedAction.LOCKED
                        );
                        gca.Execute(State);
                        command.AddAction(gca);
                    }
                }

                command.AddRange(CommonInner_UpdateFreeCards());
            }
            return command;
        }

        protected override List<GameCommandAction> CommonInner_UpdateFreeCards()
        {
            List<GameCommandAction> commands = new();
            for (int i = 0; i < TableausAmount; i++)
            {
                for (int j = 0; j < State.Tableaus[i].Count; j++)  //TOP (BACK) -->> Bottom (FRONT)
                {
                    Card currentCard = State.Tableaus[i].ElementAt(j);
                    if (currentCard.Free) continue;
                    if (!currentCard.Revealed) continue;

                    if (j == 0) //BOTTOM, FRONT-MOST CARD
                    {
                        GameCommandAction a = new GameCommandActionFree(
                             currentCard,
                             cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        commands.Add(a);
                    }
                    else if (ValidTableauCardStackToMoveAround(State.Tableaus[i].ElementAt(j - 1), currentCard))
                    {
                        GameCommandAction a = new GameCommandActionFree(
                            currentCard,
                            cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        commands.Add(a);
                    }
                    else break;
                }
            }
            return commands;
        }
    }
}