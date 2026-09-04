using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class OrthographicCameraFitter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _target;

    private Camera _cam;
    private Vector2Int _lastScreenSize;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _cam = GetComponent<Camera>();

        FitCamera();

        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastSafeArea = Screen.safeArea;
    }

    private void Update()
    {
        Vector2Int currentScreenSize =
            new Vector2Int(Screen.width, Screen.height);

        Rect currentSafeArea = Screen.safeArea;

        if (currentScreenSize != _lastScreenSize ||
            currentSafeArea != _lastSafeArea)
        {
            _lastScreenSize = currentScreenSize;
            _lastSafeArea = currentSafeArea;

            FitCamera();
        }
    }

    private void FitCamera()
    {
        if (_target == null || !_cam.orthographic)
            return;

        Bounds bounds = _target.bounds;
        Rect safeArea = Screen.safeArea;

        // Only the horizontal safe area matters.
        // The vertical safe area is completely ignored.
        float effectiveAspect =
            safeArea.width / Screen.height;

        // Calculate the orthographic size required for
        // the target to fit horizontally inside the safe area.
        float sizeForWidth =
            bounds.size.x / (2f * effectiveAspect);

        _cam.orthographicSize = sizeForWidth;

        // Position the camera horizontally so that the target
        // is centered within the horizontal safe area.
        float safeAreaCenterNormalized =
            safeArea.center.x / Screen.width;

        float cameraWorldWidth =
            _cam.orthographicSize * 2f * _cam.aspect;

        float safeAreaCenterOffset =
            (safeAreaCenterNormalized - 0.5f) *
            cameraWorldWidth;

        Vector3 position = _cam.transform.position;

        position.x =
            bounds.center.x - safeAreaCenterOffset;

        // Vertical position is based on the target, not the safe area.
        position.y = bounds.center.y;

        _cam.transform.position = position;
    }
}