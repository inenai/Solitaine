
using System.Collections;
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

        [SerializeField] GameObject _back;
        [SerializeField] GameObject _front;
        [SerializeField] TextMeshPro[] _suitStr;
        [SerializeField] TextMeshPro[] _valueStr;
        [SerializeField] TextMeshPro[] _lightAlpha;
        [SerializeField] GameObject _lockedGO;

        private Card _card;
        private IGameController _controller;
        private Vector3 _positionOnStartDrag;
        private Collider2D _collider;
        private List<TargetCardPileUI> _overlappingPiles;
        private TargetCardPileUI _targetPile;
        private bool _dragging;
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
            //RefreshDEBUG();
        }

        public void Init(IGameController controller)
        {
            _controller = controller;
        }

        public void LoadCardData(Card card, float offsetY = 0f, float offsetZ = 0f)
        {
            _card = card;
            RefreshDynamicOffset(offsetY, offsetZ);
        }

        public void LoadCardData(Card card)
        {
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

        public void RefreshDynamicOffset(float offsetY, float offsetZ)
        {
            gameObject.transform.localPosition = new Vector3(0f, offsetY, offsetZ);
            Refresh();
        }

        public void PlayLocked()
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            _lockedGO.SetActive(true);
            _lockedGO.GetComponent<SpriteRenderer>().color = new Color(1,0,0,0.5f);
            fadeCoroutine = StartCoroutine(FadeLocked());
        }

        float fadeTime;
        Coroutine fadeCoroutine;
        private IEnumerator FadeLocked()
        {
            SpriteRenderer r = _lockedGO.GetComponent<SpriteRenderer>();
            fadeTime = 0f;
            while (fadeTime < 0.5f)
            {
                float newAlpha = Mathf.Lerp(0.5f, 0f, fadeTime);
                r.color = r.color.WithAlpha(newAlpha);
                yield return null;
                fadeTime += Time.deltaTime;
            }
            _lockedGO.SetActive(false);
            fadeCoroutine = null;
        }

        private void Log(string message)
        {
            if (Card == null) Debug.Log($"[CARDUI] {message}");
            else Debug.Log($"[CARDUI][{Card}] {message}");
        }

        #region IDrag
        public bool CanDrag()
        {
            if (!Card.Free) PlayLocked();
            return Card.Free;
        }

        public void OnStartDrag()
        {
            Log("Start drag!");
            _dragging = true;
            _positionOnStartDrag = transform.position;
        }

        public void OnEndDrag()
        {
            _dragging = false;
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
            CardUI otherCard = collision.gameObject.GetComponent<CardUI>();
            if (_controller != null && otherCard != null && otherCard.Card != null)
            {
                if (_controller.IsCardInTargetPile(otherCard.Card, out TargetCardPileUI targetPile)){
                    if (!_overlappingPiles.Contains(targetPile))
                    {
                        _overlappingPiles.Add(targetPile);
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            TargetCardPileUI pile = collision.gameObject.GetComponent<TargetCardPileUI>();
            if (pile != null && _overlappingPiles.Contains(pile))
            {
                _overlappingPiles.Remove(pile);
            }
        }

        void LateUpdate()
        {
           if (_dragging) UpdateClosestTarget();
        }

        private void UpdateClosestTarget()
        {
            _targetPile = null;
            if (_overlappingPiles.Count == 0) return;

            List<TargetCardPileUI> compatiblePiles = new();
            foreach (TargetCardPileUI pile in _overlappingPiles)
            {
                bool allowedMove = pile.IsCardAllowedHere(Card);
                if (allowedMove) compatiblePiles.Add(pile);
                else pile.EnableHighlight(false);
            }

            float closestDistance = float.MaxValue;
            TargetCardPileUI closestPile = null;
            foreach (TargetCardPileUI pile in compatiblePiles)
            {
                float distance = Vector3.Distance(pile.transform.position, transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPile = pile;
                }
            }

            foreach (TargetCardPileUI pile in compatiblePiles)
            {
                if (pile == closestPile)
                {
                    _targetPile = pile;
                    pile.EnableHighlight(true);
                } else
                {
                    pile.EnableHighlight(false);
                }
            }
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
            _lockedGO.SetActive(Card != null ? !Card.Free : false);
        }
        #endregion
    }
}