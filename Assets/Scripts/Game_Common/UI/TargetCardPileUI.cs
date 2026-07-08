using Common;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TargetCardPileUI : CardPileUI
{
    [SerializeField] private GameObject triggerHighlight;

    public bool IsCardAllowedHere(Card card)
    {
        return _controller.IsCardAllowedHere(card, _pileKind, _index);
    }

    public void EnableHighlight(bool value)
    {
        triggerHighlight.SetActive(value);
    }
}
