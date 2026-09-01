using System.Collections.Generic;
using Common;
using static Utils.CommonUtils;

namespace Scorpion
{
    public class ScorpionGame : Game
    {
        public override GameState State => _state;

        public ScorpionState ScState => _state;
        protected override string DebugTag => "Scorpion";

        public override int FoundationsAmount => 4;
        public override int TableausAmount => 7;
        public override int FreeCellsAmount => 0;
        public override bool HasStock => true;
        public override bool HasWaste => false;
        protected override int DrawCount => 3;

        ScorpionState _state;

        #region initialization
        public ScorpionGame(List<Card> deck)
        {
            Log("Starting a Scorpion game.");
            CreateState();
            ShuffleAndDeal(deck);
            Log();
        }

        private void CreateState()
        {
            _state = new ScorpionState(this);
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
            //Log("Shuffling and dealing...");
            CardPile deckStack = new CardPile(Shuffle(deck.ToArray()));

            int lastTableauIndexCovered = _state.Variant == ScorpionVariant.SCORPION ? 3 : 2;

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Card nextCard = deckStack.Pop();
                    if (i > lastTableauIndexCovered || j > 2)
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

#region checks
        protected override bool ValidTableauCardStackToPlace(Card child, Card parent)
        {
            if (child == null || parent == null) return false;

            bool sameSuit = child.Suit == parent.Suit;
            return sameSuit && child.Value == parent.Value - 1;
        }

        protected override bool ValidTableauCardStackToMoveAround(Card child, Card parent)
        {
            return child.Free;
        }

        public override Card GetSolvableCard()
        {
            //Logs.Log("Spider: Looking for automatic move");

            for (int i = 0; i < TableausAmount; i++)
            {
                if (State.Tableaus[i].Count < 13) continue;

                Card validCard = null;
                int lastValue = 0;
                for (int j = 0; j < State.Tableaus[i].Count; j++)
                {
                    if (State.Tableaus[i].ElementAt(j).Value == 1)
                    {
                        validCard = State.Tableaus[i].ElementAt(j);
                        lastValue = 1;
                        continue;
                    }

                    if (validCard == null) continue;

                    if (State.Tableaus[i].ElementAt(j).Suit == validCard.Suit
                    && State.Tableaus[i].ElementAt(j).Value == lastValue + 1)
                    {
                        validCard = State.Tableaus[i].ElementAt(j);
                        lastValue = State.Tableaus[i].ElementAt(j).Value;
                    }
                    else
                    {
                        validCard = null;
                        lastValue = 0;
                    }

                    if (lastValue == 13)
                    {
                        return validCard;
                    }
                }
            }
            return null;
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
                case PileKind.FOUNDATION:
                    bool toEmptyFoundation = _state.Foundations[targetPileIndex].Count == 0;
                    return toEmptyFoundation;
                case PileKind.TABLEAU:
                    bool kingToEmpty = _state.Tableaus[targetPileIndex].Count == 0
                        && card.Value == 13;
                    bool validMove = _state.Tableaus[targetPileIndex].Count > 0 &&
                        ValidTableauCardStackToPlace(card, _state.Tableaus[targetPileIndex].Peek());
                    return kingToEmpty || validMove;
            }
            return false;
        }
        #endregion

        #region actions
        public override List<PileKind> GameAction_TrySmartMoveCard(Card card, PileData sourcePileData)
        {
            // Log($"USER GameAction_TrySmartMoveCard {card}");
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
            // Log("INNER TryDrawCardsFromStock");
            drewAmount = DrawCount;

            if (State.StockPile.Count == 0)
                return null;

            List<GameCommandAction> commands = new();
            GameCommandAction gca;

            for (int i = 0; i < DrawCount; i++)
            {
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
        #endregion
    }
}