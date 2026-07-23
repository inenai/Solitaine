namespace Common
{
    public enum SolitaireKind
    {
        KLONDIKE,
        FREECELL,
        SAWAYAMA,
        SPIDER
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

    public enum DragMode
    {
        DRAG,
        PICKUP
    }
}