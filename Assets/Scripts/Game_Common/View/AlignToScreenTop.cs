using UnityEngine;
namespace Common
{
    [ExecuteInEditMode]
    public class AlignToScreenTop : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;

        private Camera _camera;

        private void Update()
        {
            if (_anchor == null) return;
            _camera = Camera.main;
            float screenTopY = _camera.transform.position.y + _camera.orthographicSize;
            float deltaY = screenTopY - _anchor.position.y;
            transform.position += Vector3.up * deltaY;
        }
    }
}