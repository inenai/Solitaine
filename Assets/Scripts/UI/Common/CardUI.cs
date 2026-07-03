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
    IGameUIController _controller;
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

    public void Init(Card card, IGameUIController controller)
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
            txt.text = _card.Value.ToString();
        }

        foreach (TextMeshPro txt in _lightAlpha)
        {
            txt.color = txt.color.WithAlpha(0.5f);
        }

        _front.SetActive(_card.Revealed);
        _back.SetActive(!_card.Revealed);
    }

    #region IDrag
    public bool CanDrag()
    {
        return Card.Free;
    }

    public void OnStartDrag()
    {
        Debug.Log($"DraggableCardStack OnStartDrag. Original position: {transform.position}");
        _positionOnStartDrag = transform.position;
    }

    public void OnEndDrag()
    {
        Debug.Log($"DraggableCardStack OnEndDrag. Restoring saved position at {_positionOnStartDrag}");

        //TODO LOGIC
        // if (!_cardOwner.EndDrag(this))
        // {
            transform.position = _positionOnStartDrag;
        // }
    }
    #endregion

    #region IDoubleClick
    public void OnDoubleClick()
    {
        if (_card == null) return;
        _controller.CardDoubleClicked(_card);
        Debug.Log($"{Utils.CardToShortString(_card)} Double Clicked! Is controller null by any chance: {_controller == null}");
    }
    #endregion

    #region DEBUG
    public void RefreshDEBUG()
    {
        _DEBUG_DRAGGABLE.SetActive(!Card.Free);
    }
    #endregion
}
