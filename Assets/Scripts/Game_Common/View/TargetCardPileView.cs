using Common;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TargetCardPileView : CardPileView
{
    [SerializeField] protected GameObject triggerHighlight;

    public bool IsCardAllowedHere(Card card)
    {
        return _view.Controller.IsCardAllowedInPile(card, PileKind, _index);
    }

    public void EnableHighlight(bool value)
    {
        if (triggerHighlight != null) triggerHighlight.SetActive(value);
    }

/// <summary>
/// triggerHighlight will be turned on if needed in CardView's late update
/// </summary>
    void Update()
    {
        EnableHighlight(false);
    }
}
