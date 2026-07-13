using UnityEngine;
using Utils;

public class CardUISprite : CardUI
{
    private SpriteRenderer _rend;

    protected override void OnAwake()
    {
        _rend = _front.GetComponent<SpriteRenderer>();
    }

    public override void Refresh()
    {
        _rend.sprite = CardUtils.GetCardSprite(Card);
        base.Refresh();
    }
}
