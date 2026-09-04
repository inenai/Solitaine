namespace Common
{
    public enum SolitaireKind
    {
        KLONDIKE,
        FREECELL,
        SAWAYAMA,
        SPIDER,
        SCORPION,
    }

    public enum CardSuit
    {
        HEARTS,
        DIAMONDS,
        CLUBS,
        SPADES
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
        TABLEAU,
        FREECELL
    }

    public enum RevealedAction
    {
        REVEALED,
        HID
    }

    public enum FreedAction
    {
        FREED,
        LOCKED
    }

    public enum Language
    {
        ENGLISH,
        SPANISH
    }
}