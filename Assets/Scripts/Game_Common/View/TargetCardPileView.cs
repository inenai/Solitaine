using Common;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TargetCardPileView : CardPileView
{
    [SerializeField] protected GameObject triggerHighlight;

    public bool IsCardAllowedHere(Card card)
    {
        return _view.Controller.IsCardAllowedInPile(card, _pileKind, _index);
    }

    public void EnableHighlight(bool value)
    {
        triggerHighlight.SetActive(value);
    }

/// <summary>
/// triggerHighlight will be turned on if needed in CardUI's late update
/// </summary>
    void Update()
    {
        EnableHighlight(false);
    }
}
