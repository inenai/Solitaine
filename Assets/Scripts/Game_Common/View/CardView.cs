using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using UnityEngine;
using Utils;

public class CardView : MonoBehaviour, IDrag, IDoubleClick
{
    [SerializeField] protected GameObject _rotationRoot;
    [SerializeField] protected GameObject _back;
    [SerializeField] protected GameObject _front;
    [SerializeField] GameObject _lockedGO;

    protected Card _card;
    private IGameController _controller;
    private Vector3 _positionOnStartDrag;
    private List<Collider2D> _overlappingColliders;
    private SpriteRenderer _rend;
    private TargetCardPileView _targetPile;
    private bool _dragging;
    private bool _animating;

    public Card Card => _card;

    void Awake()
    {
        _positionOnStartDrag = transform.position;
        _overlappingColliders = new();
        _targetPile = null;
        _rend = _front.GetComponent<SpriteRenderer>();
    }

    public void Init(IGameController controller)
    {
        _controller = controller;
    }

    void Update()
    {
        //RefreshDEBUG();
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

    public virtual void Refresh(){
        _rend.sprite = CardUtils.GetCardSprite(Card);
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
        _lockedGO.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f);
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
            Color newColor = r.color;
            newColor.a = newAlpha;
            r.color = newColor;
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
        return !_animating && Card.Free;
    }

    public void OnDragAttemptFailed()
    {
        PlayLocked();
    }

    public void OnDoubleClickAttemptFailed()
    {
        PlayLocked();
    }

    public void OnStartDrag()
    {
        Log("Start drag!");
        _dragging = true;
        _positionOnStartDrag = transform.position;
        gameObject.layer = LayerMask.NameToLayer("DraggingCard");
    }

    public void OnEndDrag()
    {
        if (_targetPile != null)
        {
            bool successfulMove = _controller.CardDraggedToPile(Card, _targetPile.PileKind, _targetPile.Index, transform.position);
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

    private List<TargetCardPileView> GetOverlappingPiles()
    {
        List<TargetCardPileView> piles = new();
        foreach (Collider2D col in _overlappingColliders)
        {
            TargetCardPileView pile = col.gameObject.GetComponent<TargetCardPileView>();
            if (pile != null && !piles.Contains(pile))
            {
                piles.Add(pile);
                continue;
            }

            CardView otherCard = col.gameObject.GetComponent<CardView>();
            if (_controller != null && otherCard != null && otherCard.Card != null)
            {
                if (_controller.IsCardInTargetPile(otherCard.Card, out TargetCardPileView targetPile))
                {
                    if (targetPile != null && !piles.Contains(targetPile))
                    {
                        piles.Add(targetPile);
                    }
                }
            }
        }

        return piles;
    }

    #region CardToPileInteraction

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_dragging) return;

        if (!_overlappingColliders.Contains(collision)) {
            _overlappingColliders.Add(collision);
            // if (collision.GetComponent<CardUI>() != null)
            //     Debug.Log($"Colliding with {collision.GetComponent<CardUI>().Card}");
        }

        UpdateClosestTarget();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_dragging) return;

        if (_overlappingColliders.Contains(collision)) _overlappingColliders.Remove(collision);

        UpdateClosestTarget();
    }

    void LateUpdate()
    {
        if (_dragging && _overlappingColliders.Count > 0)
        {
            UpdateClosestTarget();
        }
    }

    private void UpdateClosestTarget()
    {
        _targetPile = null;
        if (_overlappingColliders.Count == 0) return;

        List<TargetCardPileView> compatiblePiles = new();
        foreach (TargetCardPileView pile in GetOverlappingPiles())
        {
            bool allowedMove = pile.IsCardAllowedHere(Card);
            if (allowedMove && !compatiblePiles.Contains(pile))
            {
                compatiblePiles.Add(pile);
            }
            else pile.EnableHighlight(false);
        }

        float closestDistance = float.MaxValue;
        TargetCardPileView closestPile = null;
        foreach (TargetCardPileView pile in compatiblePiles)
        {
            Vector2 pileXYPos = new Vector2(pile.transform.position.x, pile.transform.position.y);
            Vector2 thisXYPos = new Vector2(transform.position.x, transform.position.y);
            float distance = Vector2.Distance(pileXYPos, thisXYPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPile = pile;
            }
        }

        foreach (TargetCardPileView pile in compatiblePiles)
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
    public async Task AnimateCard(Vector3 targetPosition, float duration, bool withRevealFlip = false)
    {
        _animating = true;

        Vector3 start = new Vector3(transform.position.x, transform.position.y, -MyInputManager.DragDepth);
        Vector3 goal = new Vector3(targetPosition.x, targetPosition.y, -MyInputManager.DragDepth);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, goal, t);
            if (withRevealFlip) transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0f, 180f, 0f), t);
            await Task.Yield();
        }

        transform.position = targetPosition;
        _animating = false;
    }
    #endregion

    #region IDoubleClick

    public bool CanDoubleClick()
    {
        return !_animating && Card.Free;
    }

    public void OnDoubleClick()
    {
        Log("Double click!");
        if (_card == null) return;
        _controller.CardDoubleClicked(_card, transform.position);
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
