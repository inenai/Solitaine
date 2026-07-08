namespace Common
{
    public interface IGameController
    {
        bool CardDoubleClicked(Card card);
        bool PileClicked(PileKind pileKind, int pileIndex);
        bool IsRestockAvailable(PileKind pileKind);
        bool IsCardAllowedHere(Card card, PileKind pileKind, int pileIndex);
        bool CardDraggedToPile(Card card, PileKind pileKind, int pileIndex);
        void ResetSettingsToDefault();
    }
}