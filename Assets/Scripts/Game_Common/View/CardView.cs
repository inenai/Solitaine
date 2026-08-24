using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Utils;

public class CardView : MonoBehaviour, IDrag, IDoubleClick, IPeek
{
    private static float CardFlightSpeed = 30f;

    [SerializeField] protected GameObject _rotationRoot;
    [SerializeField] protected GameObject _back;
    [SerializeField] protected GameObject _front;
    [SerializeField] GameObject _lockedGO;

    public Card Card => _card;

    protected Card _card;
    private GameView _view;
    private Vector3 _positionOnStartDrag;
    private List<Collider2D> _overlappingColliders;
    private SpriteRenderer _rend;
    private TargetCardPileView _targetPile;
    private bool _dragging;
    private bool _animating;
    private float _fadeTime;
    private Coroutine _fadeCoroutine;
    private Coroutine _animateCardCR;
    private bool ViewRevealed => Mathf.Approximately(_rotationRoot.transform.localRotation.eulerAngles.y, 180f);
    private SpriteRenderer _frontRend;

    //AUX
    private static List<TargetCardPileView> _auxCompatiblePilesList = new();
    private static Vector2 _auxPileXYPosVector = new();
    private static Vector2 _auxThisXYPosVector = new();
    private static List<TargetCardPileView> _auxTargetPilesList = new();

    void Awake()
    {
        _frontRend = _front.GetComponent<SpriteRenderer>();
        _positionOnStartDrag = transform.position;
        _overlappingColliders = new();
        _targetPile = null;
        _rend = _front.GetComponent<SpriteRenderer>();
    }

    public void Init(GameView gameView, Card card)
    {
        _view = gameView;
        _card = card;
        UpdateView();
    }

    void LateUpdate()
    {
        if (_dragging && _overlappingColliders.Count > 0)
        {
            UpdateClosestTarget();
        }
    }

    public virtual void UpdateView()
    {
        _rend.sprite = CardUtils.GetCardSprite(Card);
        RefreshRevealedState();
    }

    public void RefreshRevealedState(){
        _rotationRoot.transform.localRotation = Quaternion.Euler(0f, Card.Revealed ? 180f : 0f, 0f);
    }

    public void RefreshDynamicOffset(float offsetY, float offsetZ)
    {
        gameObject.transform.localPosition = new Vector3(0f, offsetY, offsetZ);
    }

    private void Log(string message)
    {
        if (Card == null) Logs.Log($"[CARDUI] {message}");
        else Logs.Log($"[CARDUI][{Card}] {message}");
    }

    #region IDrag
    public bool CanDrag()
    {
        return !_animating && Card.Free;
    }

    public void OnDragAttemptFailed()
    {
        //PlayLocked();
    }

    public void OnStartDrag()
    {
        Log("Start drag!");
        _dragging = true;
        _positionOnStartDrag = transform.position;
        gameObject.layer = LayerMask.NameToLayer("DraggingCard");
    }

    public void OnEndDrag(bool cancelled)
    {
        if (_targetPile != null && !cancelled)
        {
            bool successfulMove = _view.Controller.InputAction_CardDraggedToPile(Card, _targetPile.PileKind, _targetPile.Index, transform.position);
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
        else
        {
            transform.position = _positionOnStartDrag;
        }

        //TODO if card pool, these should be reset when going to the pool:
        _targetPile = null;
        _dragging = false;
        gameObject.layer = LayerMask.NameToLayer("StaticCard");

        foreach (TargetCardPileView pile in GetOverlappingPiles())
        {
            pile.EnableHighlight(false);
        }
        _overlappingColliders.Clear();
    }
    #endregion

    #region IPeek
    int _orderInLayerBeforePeek;
    public bool StartPeeking()
    {
        if (_animating) return false;
        if (_dragging) return false;
        if (!Card.Revealed) return false;
        _orderInLayerBeforePeek = _frontRend.sortingOrder;
        _frontRend.sortingOrder += 100;
        return true;
    }

    public void StopPeeking()
    {
        _frontRend.sortingOrder = _orderInLayerBeforePeek;
    }
    #endregion

    #region IDoubleClick

    public bool CanDoubleClick()
    {
        return !_animating && Card.Free;
    }

    public void OnDoubleClickAttemptFailed()
    {
        PlayLocked();
    }

    public void OnDoubleClick()
    {
        Log($"Card {_card} double clicked!");
        if (_card == null) return;
        if (!_card.Revealed) return; //Ignores double clicks on hidden cards
        if (_view.Controller.Status != GameStatus.LISTENING) return;

        bool success = _view.Controller.InputAction_CardDoubleClicked(_card, transform.position);
        if (!success)
        {
            CardView[] cardV = transform.GetComponentsInChildren<CardView>();
            foreach (CardView c in cardV)
            {
                c.OnDoubleClickAttemptFailed();
            }
        }
    }
    #endregion

    #region CardToPileInteraction
    private List<TargetCardPileView> GetOverlappingPiles()
    {
        _auxTargetPilesList.Clear();
        foreach (Collider2D col in _overlappingColliders)
        {
            bool isTargetCardPileView = col.gameObject.TryGetComponent(out TargetCardPileView pile);

            if (isTargetCardPileView && !_auxTargetPilesList.Contains(pile))
            {
                _auxTargetPilesList.Add(pile);
                continue;
            }

            bool isCard = col.gameObject.TryGetComponent(out CardView otherCard);

            if (isCard && otherCard.Card != null)
            {
                if (_view.Controller.IsCardInTargetPile(otherCard.Card, out TargetCardPileView targetPile))
                {
                    if (targetPile != null && !_auxTargetPilesList.Contains(targetPile))
                    {
                        _auxTargetPilesList.Add(targetPile);
                    }
                }
            }
        }

        return _auxTargetPilesList;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_dragging) return;

        if (!_overlappingColliders.Contains(collision)) {
            _overlappingColliders.Add(collision);
            // if (collision.GetComponent<CardUI>() != null)
            //     Logs.Log($"Colliding with {collision.GetComponent<CardUI>().Card}");
        }

        UpdateClosestTarget();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_dragging) return;

        if (_overlappingColliders.Contains(collision)) _overlappingColliders.Remove(collision);

        UpdateClosestTarget();
    }

    private void UpdateClosestTarget()
    {
        _targetPile = null;
        if (_overlappingColliders.Count == 0) return;

        _auxCompatiblePilesList.Clear();
        foreach (TargetCardPileView pile in GetOverlappingPiles())
        {
            bool allowedMove = pile.IsCardAllowedHere(Card);
            if (allowedMove && !_auxCompatiblePilesList.Contains(pile))
            {
                _auxCompatiblePilesList.Add(pile);
            }
            else pile.EnableHighlight(false);
        }

        float closestDistance = float.MaxValue;
        TargetCardPileView closestPile = null;
        foreach (TargetCardPileView pile in _auxCompatiblePilesList)
        {
            _auxPileXYPosVector.x = pile.transform.position.x;
            _auxPileXYPosVector.y = pile.transform.position.y;
            _auxThisXYPosVector.x = transform.position.x;
            _auxThisXYPosVector.y = transform.position.y;

            float distance = Vector2.Distance(_auxPileXYPosVector, _auxThisXYPosVector);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPile = pile;
            }
        }

        foreach (TargetCardPileView pile in _auxCompatiblePilesList)
        {
            if (pile == closestPile)
            {
                _targetPile = pile;
                pile.EnableHighlight(true);
            }
            else
            {
                pile.EnableHighlight(false);
            }
        }
    }
    #endregion

    #region Animation
    public void AnimateCard(Vector3 targetPosition, float duration, Action onDone)
    {
        if (_animateCardCR != null) StopCoroutine(_animateCardCR);
        _animateCardCR = StartCoroutine(AnimateCardCR(targetPosition, duration, onDone));
    }

    public IEnumerator AnimateCardCR(Vector3 targetPosition, float duration, Action onDone)
    {
        Logs.Log($"Card {_card} animating...");
        _animating = true;

        Vector3 start = new Vector3(transform.position.x, transform.position.y, -MyInputManager.DragDepth);
        Vector3 goal = new Vector3(targetPosition.x, targetPosition.y, -MyInputManager.DragDepth);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, goal, t);
            if (!ViewRevealed && Card.Revealed)
                _rotationRoot.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 180f, 0f), t);
            yield return null;
        }

        transform.position = targetPosition;
        RefreshRevealedState();

        _animating = false;
        Logs.Log($"Card {_card} animated.");
        _animateCardCR = null;
        onDone?.Invoke();
    }

    public void PlayLocked()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _lockedGO.SetActive(true);
        _lockedGO.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
        _fadeCoroutine = StartCoroutine(FadeLocked());
    }

    private IEnumerator FadeLocked()
    {
        SpriteRenderer r = _lockedGO.GetComponent<SpriteRenderer>();
        _fadeTime = 0f;
        while (_fadeTime < 0.5f)
        {
            float newAlpha = Mathf.Lerp(0.5f, 0f, _fadeTime);
            Color newColor = r.color;
            newColor.a = newAlpha;
            r.color = newColor;
            yield return null;
            _fadeTime += Time.deltaTime;
        }
        _lockedGO.SetActive(false);
        _fadeCoroutine = null;
    }

    Coroutine _revealCoroutine;
    public void PlayRevealIfNeeded()
    {
        if (ViewRevealed && !Card.Revealed)
        {
            RefreshRevealedState();
            return;
        }

        if (!ViewRevealed && Card.Revealed)
        {
            if (_revealCoroutine != null) StopCoroutine(_revealCoroutine);
            _revealCoroutine = StartCoroutine(PlayRevealCR());
        }
    }

    private IEnumerator PlayRevealCR()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (!ViewRevealed && Card.Revealed)
                _rotationRoot.transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 180f, 0f), t);
            yield return null;
        }
        RefreshRevealedState();
        _revealCoroutine = null;
    }

    public static float GetFlightTime(Vector3 origin, Vector3 destiny)
    {
        float distance = Vector3.Distance(origin, destiny);
        if (Mathf.Approximately(distance, 0f))
            return 0.01f;
        return distance / CardFlightSpeed;
    }
    #endregion


}
