using UnityEngine;
using UnityEngine.InputSystem;

namespace Common
{
   public class MyInputManager : MonoBehaviour
   {
      [Header("Input References")]
      [SerializeField] private InputActionReference pointerMovedAction;
      [SerializeField] private InputActionReference pointerDownAction;
      [SerializeField] private InputActionReference doublePressAction;

      [SerializeField] private float mouseDragSpeed = 0.1f;
      [SerializeField] private float dragDepth = 5f;

      private Vector3 _dragOffset;
      private Camera _mainCamera;
      private Plane _dragPlane;
      private Vector3 _velocity = Vector3.zero;
      private GameObject _draggingObject;
      private GameObject _clickingObject;
      private Vector3 _pointerPosition;
      private bool dragging => _draggingObject != null;
      private bool clicking => _clickingObject != null;

      private Collider2D _previousClickedCollider;
      private Collider2D _lastClickCollider;

      private void Awake()
      {
         _mainCamera = Camera.main;
         _dragPlane = new Plane(Vector3.forward, dragDepth);
      }

      private void OnEnable()
      {
         pointerMovedAction.action.Enable();
         pointerDownAction.action.Enable();
         doublePressAction.action.Enable();

         pointerDownAction.action.performed += PointerPressed;
         pointerMovedAction.action.performed += PointerMoved;
         pointerDownAction.action.canceled += PointerReleased;
         doublePressAction.action.performed += DoublePressed;
      }

      private void OnDisable()
      {
         pointerMovedAction.action.Disable();
         pointerDownAction.action.Disable();
         doublePressAction.action.Disable();

         pointerDownAction.action.performed -= PointerPressed;
         pointerMovedAction.action.performed -= PointerMoved;
         pointerDownAction.action.canceled -= PointerReleased;
         doublePressAction.action.performed -= DoublePressed;
      }

      private void PointerPressed(InputAction.CallbackContext context)
      {
         // Debug.Log("[InputManager] Pointer pressed");
         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            // Debug.Log("[InputManager] Collider hit!");
            _previousClickedCollider = _lastClickCollider;
            _lastClickCollider = hit.collider;
            bool dragAvailable = TryBeginDrag(hit.collider);
            bool clickAvailable = TryBeginClick(hit.collider);
            if (!(dragAvailable || clickAvailable))
            {
               CardUI targetCard = hit.collider.GetComponent<CardUI>();
               if (targetCard != null)
                  targetCard.PlayLocked();
            }
         }
      }

      private void PointerMoved(InputAction.CallbackContext context)
      {
         //Debug.Log("[InputManager] Pointer moved");
         _pointerPosition = context.ReadValue<Vector2>();
         if (dragging)
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            if (_dragPlane.Raycast(ray, out float distance))
            {
               _draggingObject.transform.position = Vector3.SmoothDamp(_draggingObject.transform.position, ray.GetPoint(distance), ref _velocity, mouseDragSpeed) + _dragOffset;
            }
         }
      }

      private void PointerReleased(InputAction.CallbackContext context)
      {
         // Debug.Log("[InputManager] Pointer released");
         if (dragging)
         {
            _draggingObject.GetComponent<IDrag>()?.OnEndDrag();
            _draggingObject = null;
            Debug.Log("[InputManager] Drag ended");
         }

         if (clicking)
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == _clickingObject)
            {
              _clickingObject.GetComponent<IClick>()?.OnClick();
            }
            _clickingObject = null;
            Debug.Log("[InputManager] Click ended (happened)");
         }
      }

      private void DoublePressed(InputAction.CallbackContext context)
      {
         Debug.Log("[InputManager] Pointer double pressed");
         _draggingObject = null; //Cancel drag

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            IDoubleClick dc = hit.collider.gameObject.GetComponent<IDoubleClick>();
            if (dc != null && _lastClickCollider == _previousClickedCollider && dc.CanDoubleClick())
            {
               dc.OnDoubleClick();
            }
         }
         _previousClickedCollider = null;
         _lastClickCollider = null;
      }

      private bool TryBeginDrag(Collider2D collider)
      {
         IDrag dragComponent = collider.gameObject.GetComponent<IDrag>();
         if (dragComponent != null && dragComponent.CanDrag())
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            if (_dragPlane.Raycast(ray, out float distance))
            {
               Vector3 fullOffset = collider.transform.position - ray.GetPoint(distance);
               _dragOffset = new Vector3(fullOffset.x, fullOffset.y, 0f);
            }
            _draggingObject = collider.gameObject;
            dragComponent.OnStartDrag();
            Debug.Log("[InputManager] Drag started");
            return true;
         }
         return false;
      }

      private bool TryBeginClick(Collider2D collider)
      {
         IClick click = collider.gameObject.GetComponent<IClick>();
         if (click != null)
         {
            _clickingObject = collider.gameObject;
            Debug.Log("[InputManager] Click started");
            return true;
         }
         return false;
      }
   }
}