using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Common
{
    [RequireComponent(typeof(Collider2D))]
    public class CardUI : MonoBehaviour, IDrag, IDoubleClick
    {
        private const float _offsetY = -0.3f;
        private const float _offsetZ = -0.1f;

        [SerializeField] GameObject _back;
        [SerializeField] GameObject _front;
        [SerializeField] TextMeshPro[] _suitStr;
        [SerializeField] TextMeshPro[] _valueStr;
        [SerializeField] TextMeshPro[] _lightAlpha;

        private Card _card;
        private IGameController _controller;
        private Vector3 _positionOnStartDrag;
        private Collider2D _collider;
        private List<TargetCardPileUI> _overlappingPiles;
        private TargetCardPileUI _targetPile;

        //DEBUG
        [SerializeField] GameObject _DEBUG_DRAGGABLE;

        public Card Card => _card;

        void Awake()
        {
            _positionOnStartDrag = transform.position;
            _collider = GetComponent<Collider2D>();
            _overlappingPiles = new();
            _targetPile = null;
        }

        void Update()
        {
            RefreshDEBUG();
        }

        public void Init(IGameController controller)
        {
            _controller = controller;
        }

        public void LoadCardData(Card card, int index = -1)
        {
            _card = card;
            Refresh(index);
        }

        public void Reveal(bool reveal)
        {
            _card.Show(reveal);
            _front.SetActive(_card.Revealed);
            _back.SetActive(!_card.Revealed);
        }

        private Vector3 GetCardOffset(int index)
        {
            float offsetY = _offsetY * index;
            float offsetZ = _offsetZ + (_offsetZ * index);
            return new Vector3(0f, offsetY, offsetZ);
        }

        public void Refresh(int index)
        {
            if (index >= 0)
            {
                Vector3 offset = GetCardOffset(index);
                gameObject.transform.localPosition = offset;
            }

            foreach (TextMeshPro txt in _suitStr)
            {
                txt.text = CardUtils.GetSuitStr(_card.Suit);
                txt.color = CardUtils.GetSuitColor(_card.Suit);
            }

            foreach (TextMeshPro txt in _valueStr)
            {
                txt.text = CardUtils.CardValue(_card);
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
            else Debug.Log($"[CARDUI][{Card}] {message}");
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
            if (_card == null) return;

            bool successfulMove = false;
            if (_targetPile != null)
            {
                successfulMove = _controller.CardDraggedToPile(Card, _targetPile.PileKind, _targetPile.Index);
            }

            if (!successfulMove)
            {
                Log($"End drag! Restoring saved position at {_positionOnStartDrag}");
                transform.position = _positionOnStartDrag;
            }
            else
            {
                Log("End drag! Sent card to target pile.");
            }

        }
        #endregion

        #region CardToPileInteraction

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TargetCardPileUI pile = collision.gameObject.GetComponent<TargetCardPileUI>();
            if (pile != null && !_overlappingPiles.Contains(pile))
            {
                _overlappingPiles.Add(pile);
            }
            UpdateClosestTarget();
        }

        private void UpdateClosestTarget()
        {
            _targetPile = null;
            if (_overlappingPiles.Count == 0) return;

            float closestDistance = float.MaxValue;
            TargetCardPileUI closestPile = null;
            foreach (TargetCardPileUI pile in _overlappingPiles)
            {
                float distance = Vector3.Distance(pile.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPile = pile;
                }
            }

            foreach (TargetCardPileUI pile in _overlappingPiles)
            {
                bool allowedMove = pile.IsCardAllowedHere(Card);
                if (pile == closestPile && allowedMove)
                {
                    _targetPile = pile;
                }

                pile.EnableHighlight(pile == _targetPile);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            TargetCardPileUI pile = collision.gameObject.GetComponent<TargetCardPileUI>();
            if (pile != null && _overlappingPiles.Contains(pile))
            {
                _overlappingPiles.Remove(pile);
            }
            UpdateClosestTarget();
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
            Log($"{_card} Double Clicked!");
        }
        #endregion

        #region DEBUG
        public void RefreshDEBUG()
        {
            _DEBUG_DRAGGABLE.SetActive(Card != null ? !Card.Free : false);
        }
        #endregion
    }
}