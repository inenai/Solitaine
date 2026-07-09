using System.Threading.Tasks;

namespace Common
{
    public interface IGameController
    {
        bool CardDraggedToPile(Card card, PileKind pileKind, int pileIndex);
        bool CardDoubleClicked(Card card);
        bool PileClicked(PileKind pileKind, int index);
        bool IsRestockAvailable(PileKind pileKind);
        bool IsCardAllowedHere(Card card, PileKind pileKind, int pileIndex);
        void ResetSettingsToDefault();
        bool IsCardInTargetPile(Card card, out TargetCardPileUI targetPile);
    }
}