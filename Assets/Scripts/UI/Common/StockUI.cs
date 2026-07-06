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
    private IGameController _controller;

    public void Init(Stack<Card> stock, IGameController controller, Action onDone = null)
    {
        _controller = controller;
        Refresh(stock,onDone);
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
