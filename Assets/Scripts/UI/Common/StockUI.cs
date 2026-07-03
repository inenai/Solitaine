using System;
using System.Collections.Generic;
using Model.Common;
using UI.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using static Model.Common.Enums;

[RequireComponent(typeof(Collider2D))]
public class StockUI : MonoBehaviour
{
    Collider2D _collider2d;
    IGameUIController _master;
    [SerializeField] CardUI _cardUI;
    [SerializeField] protected PileKind _pileKind;

    void Awake()
    {
        _collider2d = GetComponent<Collider2D>();
    }

    public void Init(IGameUIController master)
    {
        _master = master;
    }

    public void Refresh(Stack<Card> stock, Action onDone)
    {
        _cardUI.gameObject.SetActive(stock.Count > 0);
        if (stock.Count > 0)
        {
            _cardUI.Init(stock.Peek());
        }
        onDone?.Invoke();
    }

    void Update()
    {
        // ProcessClicks();
    }

    // private void ProcessClicks()
    // {
    //     if (!Mouse.current.leftButton.wasReleasedThisFrame)
    //         return;

    //     Vector2 mousePos =
    //         Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    //     Collider2D hit = Physics2D.OverlapPoint(mousePos);

    //     if (hit == _collider2d)
    //     {
    //         LogClicked();
    //         _master.PilePressed(_pileKind);
    //     }
    // }

    protected virtual void LogPressed()
    {
        Debug.Log($"{_pileKind} pile pressed!");
    }
}
