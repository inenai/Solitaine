using System.Collections;
using UnityEngine;

public class HamburgerBtn : MonoBehaviour
{
    [SerializeField] private RectTransform _animationObject;
    [SerializeField] private float _duration = 0.5f;

    private const string MENU_ID = "MENU_Hamburger";

    private float _animationXOrigin;
    private bool _animating => _animationCR != null;
    private Coroutine _animationCR;
    private bool _showing;

    void Awake()
    {
        _animationXOrigin = _animationObject.anchoredPosition.x;
    }

    public void AnimateMenu()
    {
        if (_animating) return;
        _animationCR = StartCoroutine(ShowAnim(!_showing));
    }

    private IEnumerator ShowAnim(bool show)
    {
        if (show) EventManager.OnMenuOpened(MENU_ID);

        Vector2 start = new Vector2(_animationXOrigin, _animationObject.anchoredPosition.y);
        Vector2 goal = new Vector2(0f, _animationObject.anchoredPosition.y);

        if (!show)
        {
            start = goal;
            goal = new Vector2(_animationXOrigin, _animationObject.anchoredPosition.y);
        }

        float elapsed = 0f;
        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _duration);
            _animationObject.anchoredPosition = Vector2.Lerp(start, goal, t);
            yield return null;
        }

        _animationObject.anchoredPosition = goal;
        _showing = show;
        _animationCR = null;

        if (!show) EventManager.OnMenuClosed(MENU_ID);
    }
}
