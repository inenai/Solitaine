using UnityEngine;

namespace Common
{
    public interface IGameController
    {
        void RestartGame();
        bool InputAction_CardDraggedToPile(Card card, PileKind pileKind, int pileIndex, Vector3 originalCardPosition);
        bool InputAction_CardDoubleClicked(Card card, Vector3 originalCardPosition);
        bool InputAction_PileClicked(PileKind pileKind, int pileIndex);
        bool IsRestockAvailable();
        bool IsAutoMovesEnabled();
        bool IsCardAllowedInPile(Card card, PileKind pileKind, int pileIndex);
        void ResetSettingsToDefault();
        bool IsCardInTargetPile(Card card, out TargetCardPileView targetPile);
    }
}