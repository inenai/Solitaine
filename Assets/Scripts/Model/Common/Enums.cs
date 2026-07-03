using UnityEngine;

namespace Model.Common
{
    public class Enums
    {
        public enum SolitaireKind
        {
            KLONDIKE,
            FREECELL,
            SAWAYAMA,
            SPIDER
        }

        public enum Suit
        {
            HEARTS,
            DIAMONDS,
            CLUBS,
            SPADES,
            ANY
        }

        public enum Status
        {
            INITIALIZING,
            LISTENING,
            PROCESSING
        }

        public enum GameStatus
        {
            INITIALIZING,
            LISTENING,
            PROCESSING
        }

        public enum PileKind
        {
            WASTE,
            STOCK,
            FOUNDATION,
            TABLEAU
        }

    }
}