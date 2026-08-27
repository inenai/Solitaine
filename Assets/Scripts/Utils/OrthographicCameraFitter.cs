using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthographicCameraFitter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _target;

    private Camera _cam;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        FitCamera();
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }

    private void Update()
    {
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (currentScreenSize != _lastScreenSize)
        {
            _lastScreenSize = currentScreenSize;
            FitCamera();
        }
    }

    private void FitCamera()
    {
        if (_target == null || !_cam.orthographic)
            return;

        Bounds bounds = _target.bounds;

        float sizeForHeight = bounds.size.y / 2f;
        float sizeForWidth = bounds.size.x / (2f * _cam.aspect);

        _cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

        Vector3 position = _cam.transform.position;
        position.x = bounds.center.x;
        position.y = bounds.center.y;
        _cam.transform.position = position;
    }
}