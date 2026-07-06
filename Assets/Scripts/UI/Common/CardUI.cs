using Common.Utils;
using Common.Input;
using Model.Common;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UI.Common;

[RequireComponent(typeof(Collider2D))]
public class CardUI : MonoBehaviour, IDrag, IDoubleClick
{
    Card _card;
    IGameController _controller;
    private Vector3 _positionOnStartDrag;
    private Collider2D _collider;

    [SerializeField] GameObject _back;
    [SerializeField] GameObject _front;
    [SerializeField] TextMeshPro[] _suitStr;
    [SerializeField] TextMeshPro[] _valueStr;
    [SerializeField] TextMeshPro[] _lightAlpha;

    //DEBUG
    [SerializeField] GameObject _DEBUG_DRAGGABLE;

    public Card Card => _card;

    void Awake()
    {
        _positionOnStartDrag = transform.position;
        _collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        RefreshDEBUG();
    }

    public void Init(Card card, IGameController controller)
    {
        _controller = controller;
        _card = card;
        Refresh();
    }

    public void Reveal(bool reveal)
    {
        _card.Show(reveal);
        _front.SetActive(_card.Revealed);
        _back.SetActive(!_card.Revealed);
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
            txt.text = Utils.CardValue(_card);
        }

        foreach (TextMeshPro txt in _lightAlpha)
        {
            txt.color = txt.color.WithAlpha(0.5f);
        }

        _front.SetActive(_card.Revealed);
        _back.SetActive(!_card.Revealed);
    }

    private void Log(string message)
    {
        if (Card == null) Debug.Log($"[CARDUI] {message}");
        else Debug.Log($"[CARDUI][{Utils.CardToShortString(Card)}] {message}");
    }

    #region IDrag
    public bool CanDrag()
    {
        return Card.Free;
    }

    public void OnStartDrag()
    {
        Log("Start drag!");
        _positionOnStartDrag = transform.position;
    }

    public void OnEndDrag()
    {
        Log("End drag!");
        if (_card == null) return;
        //TODO LOGIC
        // if (!_cardOwner.EndDrag(this))
        // {
        Log($"DraggableCardStack OnEndDrag. Restoring saved position at {_positionOnStartDrag}");
        transform.position = _positionOnStartDrag;
        // }
    }
    #endregion

    #region IDoubleClick

    public bool CanDoubleClick()
    {
        return Card.Free;
    }

    public void OnDoubleClick()
    {
        Log("Double click!");
        if (_card == null) return;
        _controller.CardDoubleClicked(_card);
        Log($"{Utils.CardToShortString(_card)} Double Clicked!");
    }
    #endregion

    #region DEBUG
    public void RefreshDEBUG()
    {
        _DEBUG_DRAGGABLE.SetActive(!Card.Free);
    }
    #endregion
}
