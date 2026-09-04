using UnityEngine;

namespace Common
{
    public class ScaleSpriteToScreenBottom : MonoBehaviour
    {
        private float _offset = 0.6f;

        private SpriteRenderer _spriteRenderer;
        private Camera _camera;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_spriteRenderer == null || _camera == null)
                return;

            Bounds bounds = _spriteRenderer.bounds;

            // World-space Y coordinate of the bottom of the screen.
            float screenBottom = _camera.ScreenToWorldPoint(
                new Vector3(
                    0f,
                    0f,
                    Mathf.Abs(
                        transform.position.z -
                        _camera.transform.position.z
                    )
                )
            ).y;

            float targetBottom = screenBottom + _offset;

            // The top of the sprite remains where it currently is.
            float top = bounds.max.y;

            float targetHeight = top - targetBottom;
            float currentHeight = bounds.size.y;

            if (currentHeight <= 0f || targetHeight <= 0f)
                return;

            float scaleFactor = targetHeight / currentHeight;

            Vector3 scale = transform.localScale;
            scale.y *= scaleFactor;
            transform.localScale = scale;
        }
    }
}