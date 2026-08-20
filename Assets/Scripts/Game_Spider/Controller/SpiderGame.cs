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
            if (targetPile != PileKind.TABLEAU)
                return false;
            if (sourcePileData.Index == targetPileIndex)
                return false;

            switch (targetPile)
            {
                case PileKind.TABLEAU:
                    bool toEmptyTableau = _state.Tableaus[targetPileIndex].Count == 0;
                    bool validStack = !toEmptyTableau && ValidTableauCardStackToPlace(card, _state.Tableaus[targetPileIndex].Peek());

                    return toEmptyTableau || validStack;
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
                    if (State.Tableaus[i].ListCopy[j].Value == 13)
                    {
                        validCard = State.Tableaus[i].ListCopy[j];
                        lastValue = 13;
                        continue;
                    }

                    if (validCard == null) continue;

                    if (State.Tableaus[i].ListCopy[j].Suit == validCard.Suit
                    && State.Tableaus[i].ListCopy[j].Value == lastValue - 1)
                    {
                        lastValue = State.Tableaus[i].ListCopy[j].Value;
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

            Queue<GameCommandAction> commands = new();
            GameCommandAction gca = null;

            for (int i = 0; i < TableausAmount; i++)
            {
                foreach (Card card in State.Tableaus[i].ListCopy)
                {
                    if (card.Free)
                    {
                        gca = new GameCommandActionFree(
                            card,
                            cardFreed: FreedAction.LOCKED
                        );
                        gca.Execute(State);
                        commands.Enqueue(gca);
                    }
                }

                gca = new GameCommandActionMove(
                       sourcePile: PileKind.STOCK,
                       targetPile: PileKind.TABLEAU, targetIndex: i
                  );
                gca.Execute(State);
                commands.Enqueue(gca);

                gca = new GameCommandActionReveal(
                    State.Tableaus[i].Peek(),
                    cardRevealed: RevealedAction.REVEALED
                );
                gca.Execute(State);
                commands.Enqueue(gca);

                gca = new GameCommandActionFree(
                  State.Tableaus[i].Peek(),
                  cardFreed: FreedAction.FREED
                );

                gca.Execute(State);
                commands.Enqueue(gca);
            }

            Queue<GameCommandAction> extraCommands = CommonInner_UpdateFreeCards();
            while (extraCommands.Count > 0)
            {
                commands.Enqueue(extraCommands.Dequeue());
            }

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
                foreach (Card tcard in State.Tableaus[targetPileIndex].ListCopy)
                {
                    if (tcard != card && tcard.Free)
                    {
                        GameCommandAction gca = new GameCommandActionFree(
                            tcard,
                            cardFreed: FreedAction.LOCKED
                        );
                        gca.Execute(State);
                        command.Enqueue(gca);
                    }
                }

                Queue<GameCommandAction> extraCommands = CommonInner_UpdateFreeCards();
                while (extraCommands.Count > 0)
                {
                    command.Enqueue(extraCommands.Dequeue());
                }
            }
            return command;
        }

        protected override Queue<GameCommandAction> CommonInner_UpdateFreeCards()
        {
            Queue<GameCommandAction> commands = new();
            for (int i = 0; i < TableausAmount; i++)
            {
                for (int j = State.Tableaus[i].Count - 1; j >= 0; j--)
                {
                    Card currentCard = State.Tableaus[i].ListCopy[j];
                    if (currentCard.Free) continue;
                    if (!currentCard.Revealed) continue;

                    if (j == State.Tableaus[i].Count - 1)
                    {
                        GameCommandAction a = new GameCommandActionFree(
                             currentCard,
                             cardFreed: FreedAction.FREED);
                        a.Execute(State);
                        commands.Enqueue(a);
                    }
                    else
                    {
                        if (ValidTableauCardStackToMoveAround(State.Tableaus[i].ListCopy[j + 1], currentCard))
                        {
                            GameCommandAction a = new GameCommandActionFree(
                                currentCard,
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
    }
}