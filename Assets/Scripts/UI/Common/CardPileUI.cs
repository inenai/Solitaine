using System;
using UI.Common;
using UnityEngine;
using static Model.Common.Enums;

public abstract class CardPileUI : MonoBehaviour
{
    [SerializeField] protected PileKind _pileKind;
    protected int _index = -1;
    protected IGameUIController _controller;
    public PileKind PileKind => _pileKind;
    public int Index => _index;

    public void Init(IGameUIController controller, int index = -1, Action onDone = null)
    {
        _controller = controller;
        _index = index;
    }
}