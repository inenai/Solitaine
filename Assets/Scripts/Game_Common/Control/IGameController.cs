namespace Common
{
    public interface IGameController
    {
        bool CardDoubleClicked(Card card);
        bool PileClicked(PileKind pileKind, int index);
        bool IsRestockAvailable(PileKind pileKind);
    }
}