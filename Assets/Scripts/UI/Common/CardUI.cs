using Common;
using Model.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class CardUI : MonoBehaviour
{
    private const float DoubleClickTime = 0.3f;
    private float _lastClickTime = -1f;

    Card _card;
    KlondikeTableauUI _masterTableau;
    IDrag dragComponent;

    [SerializeField] GameObject _back;
    [SerializeField] GameObject _front;
    [SerializeField] TextMeshPro[] _suitStr;
    [SerializeField] TextMeshPro[] _valueStr;

    [SerializeField] TextMeshPro[] _lightAlpha;

    public Card Card => _card;
    private bool Draggable => dragComponent != null;

    void Awake()
    {
        TryGetComponent(out dragComponent);
    }

    public void Init(Card card, KlondikeTableauUI master)
    {
        _masterTableau = master;
        Refresh(card);
    }

    public void Init(Card card)
    {
        Refresh(card);
    }

    public void Reveal(bool reveal)
    {
        _card.Show(reveal);
        _front.SetActive(_card.Revealed);
        _back.SetActive(!_card.Revealed);

        if (Draggable)
        {
            dragComponent.EnableDrag(reveal);
        }
    }

    private void Refresh(Card card)
    {
        _card = card;
        Refresh();
    }

    public void Refresh()
    {
        foreach (TextMeshPro txt in _suitStr)
        {
            txt.text = Utils.GetSuitStr(_card.Suit);
            txt.color = Utils.GetSuitColor(_card.Suit);
        }

        foreach (TextMeshPro txt in _valueStr)
        {
            txt.text = _card.Value.ToString();
        }

        foreach (TextMeshPro txt in _lightAlpha)
        {
            txt.color = txt.color.WithAlpha(0.5f);
        }

        _front.SetActive(_card.Revealed);
        _back.SetActive(!_card.Revealed);
    }


    void Update()
    {
        // if (_masterTableau != null) ProcessClicks();
    }

    // private void ProcessClicks()
    // {
    //     if (!Mouse.current.leftButton.wasReleasedThisFrame)
    //         return;

    //     Vector2 mousePos =
    //         Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

    //     Collider2D hit = Physics2D.OverlapPoint(mousePos);

    //     if (hit != _collider2d)
    //         return;

    //     float currentTime = Time.time;

    //     if (currentTime - _lastClickTime <= DoubleClickTime)
    //     {
    //         _lastClickTime = -1f; // Reset so a triple click doesn't trigger twice
    //         OnDoubleClick();
    //     }
    //     else
    //     {
    //         _lastClickTime = currentTime;
    //     }
    // }

    protected virtual void OnDoubleClick()
    {
        if (_card == null) return;
        _masterTableau.CardDoublePressed();
        Debug.Log($"{_card.Value}({_card.Suit}) Double Clicked!");
    }
}
