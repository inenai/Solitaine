using UnityEngine;

namespace Common
{
    public interface IGameController
    {
        void RestartGame();
        bool CardDraggedToPile(Card card, PileKind pileKind, int pileIndex, Vector3 originalCardPosition);
        bool CardDoubleClicked(Card card, Vector3 originalCardPosition);
        bool PileClicked(PileKind pileKind, int pileIndex);
        bool IsRestockAvailable(PileKind pileKind);
        bool IsCardAllowedInPile(Card card, PileKind pileKind, int pileIndex);
        void ResetSettingsToDefault();
        bool IsCardInTargetPile(Card card, out TargetCardPileView targetPile);
    }
}