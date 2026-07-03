using System;
using System.Collections.Generic;
using Common.Input;
using Model.Common;
using UI.Common;
using UnityEngine;
using static Model.Common.Enums;

[RequireComponent(typeof(Collider2D))]
public class StockUI : MonoBehaviour, IClick
{
    [SerializeField] GameObject _cardUI;
    private IGameUIController _controller;

    public void Init(IGameUIController controller)
    {
        _controller = controller;
    }

    public void Refresh(Stack<Card> stock, Action onDone)
    {
        _cardUI.gameObject.SetActive(stock.Count > 0);
        onDone?.Invoke();
    }

    public void OnClick()
    {
        Debug.Log($"Stock pile clicked!");
        _controller.PileClicked(PileKind.STOCK,-1);
    }
}
