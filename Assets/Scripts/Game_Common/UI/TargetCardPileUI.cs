using Common;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class TargetCardPileUI : CardPileUI
{
    [SerializeField] private GameObject triggerHighlight;

    void OnTriggerEnter2D(Collider2D collision)
    {
        CardUI cardUI = collision.gameObject.GetComponent<CardUI>();
        if (cardUI != null && _controller.IsCardAllowedHere(cardUI.Card, _pileKind, _index))
        {
            triggerHighlight.SetActive(true);
        }

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        triggerHighlight.SetActive(false);
    }
}
