using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthographicCameraFitter : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;

    private Camera cam;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        FitCamera();
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }

    private void Update()
    {
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (currentScreenSize != lastScreenSize)
        {
            lastScreenSize = currentScreenSize;
            FitCamera();
        }
    }

    private void FitCamera()
    {
        if (target == null || !cam.orthographic)
            return;

        Bounds bounds = target.bounds;

        float sizeForHeight = bounds.size.y / 2f;
        float sizeForWidth = bounds.size.x / (2f * cam.aspect);

        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);

        Vector3 position = cam.transform.position;
        position.x = bounds.center.x;
        position.y = bounds.center.y;
        cam.transform.position = position;
    }
}