using UnityEngine;
using UnityEngine.InputSystem;

namespace Common
{
   public class MyInputManager : MonoBehaviour
   {
      [Header("Settings")]
      [SerializeField] private bool dragEnabled;
      [SerializeField] private bool pickUpEnabled;
      [SerializeField] private bool doubleClickEnabled;

      [Header("Input References")]
      [SerializeField] private InputActionReference pointerMovedAction;
      [SerializeField] private InputActionReference pointerDownAction;
      [SerializeField] private InputActionReference doublePressAction;
      [SerializeField] private InputActionReference resetGameAction;
      [SerializeField] private InputActionReference drawFromStockAction;

      [SerializeField] private float mouseDragSpeed = 0.1f;

      public const float DragDepth = 5f;

      private bool _paused;
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
         _dragPlane = new Plane(Vector3.forward, DragDepth);
      }

      private void OnEnable()
      {
         pointerMovedAction.action.Enable();
         pointerDownAction.action.Enable();
         resetGameAction.action.Enable();
         drawFromStockAction.action.Enable();

         pointerDownAction.action.performed += Action_PointerPressed;
         pointerMovedAction.action.performed += Action_PointerMoved;
         pointerDownAction.action.canceled += Action_PointerReleased;
         resetGameAction.action.performed += Action_ResetGame;
         drawFromStockAction.action.performed += Action_DrawFromStock;

         if (doubleClickEnabled)
         {
            doublePressAction.action.Enable();
            doublePressAction.action.performed += Action_DoublePressed;
         }
      }

      private void OnDisable()
      {
         pointerMovedAction.action.Disable();
         pointerDownAction.action.Disable();
         resetGameAction.action.Disable();
         drawFromStockAction.action.Disable();

         pointerDownAction.action.performed -= Action_PointerPressed;
         pointerMovedAction.action.performed -= Action_PointerMoved;
         pointerDownAction.action.canceled -= Action_PointerReleased;
         resetGameAction.action.performed -= Action_ResetGame;
         drawFromStockAction.action.performed -= Action_DrawFromStock;

         if (doubleClickEnabled)
         {
            doublePressAction.action.Disable();
            doublePressAction.action.performed -= Action_DoublePressed;
         }
      }

      public void Pause(bool pause)
      {
         _paused = pause;
      }

      private void Action_ResetGame(InputAction.CallbackContext context)
      {
         if (dragging) return;
         EventManager.OnResetGameEvent?.Invoke();
      }

      private void Action_DrawFromStock(InputAction.CallbackContext context)
      {
         if (dragging) return;
         EventManager.OnDrawFromStockEvent?.Invoke();
      }

      private void Action_PointerPressed(InputAction.CallbackContext context)
      {
         // Debug.Log("[InputManager] Pointer pressed");
         if (_paused) return;

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            // Debug.Log("[InputManager] Collider hit!");
            _previousClickedCollider = _lastClickCollider;
            _lastClickCollider = hit.collider;

            if (dragging && pickUpEnabled)
            {
               _draggingObject.GetComponent<IDrag>()?.OnEndDrag();
               _draggingObject = null;
               Debug.Log("[InputManager] Drag ended");
               return;
            }

            if (dragEnabled)
            {
               TryBeginDrag(hit.collider);
            }
            TryBeginClick(hit.collider);
         }
      }

      private void Action_PointerMoved(InputAction.CallbackContext context)
      {
         //Debug.Log("[InputManager] Pointer moved");
         if (_paused) return;

         _pointerPosition = context.ReadValue<Vector2>();
         if (dragging)
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            if (_dragPlane.Raycast(ray, out float distance))
            {
               _draggingObject.transform.position = Vector3.SmoothDamp(
                  current: _draggingObject.transform.position,
                  target: ray.GetPoint(distance),
                  currentVelocity: ref _velocity,
                  smoothTime: mouseDragSpeed) + _dragOffset;
            }
         }
      }

      private void Action_PointerReleased(InputAction.CallbackContext context)
      {
         // Debug.Log("[InputManager] Pointer released");
         if (_paused) return;

         if (dragging && !pickUpEnabled)
         {
            _draggingObject.GetComponent<IDrag>()?.OnEndDrag();
            _draggingObject = null;
            Debug.Log("[InputManager] Drag ended");
         }

         if (clicking)
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
               IClick c = hit.collider.gameObject.GetComponent<IClick>();
               bool clickable = c != null;

               if (clickable)
               {
                  //ICLICK
                  if (hit.collider.gameObject == _clickingObject)
                  {
                     if (c.CanClick())
                     {
                        c.OnClick();
                     }
                     else
                     {
                        c.OnClickAttemptFailed();
                     }
                  }
               } else if (pickUpEnabled)
               {
                  TryBeginDrag(hit.collider);
               }
            }
            _clickingObject = null;
            Debug.Log("[InputManager] Click ended (happened)");
         }
      }

      private void Action_DoublePressed(InputAction.CallbackContext context)
      {
         Debug.Log("[InputManager] Pointer double pressed");
         if (_paused) return;

         _draggingObject = null; //Cancel drag

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            IDoubleClick dc = hit.collider.gameObject.GetComponent<IDoubleClick>();
            if (dc != null && _lastClickCollider == _previousClickedCollider)
            {
               if (dc.CanDoubleClick())
               {
                  dc.OnDoubleClick();
               }
               else
               {
                  dc.OnDoubleClickAttemptFailed();
               }
            }
         }
         _previousClickedCollider = null;
         _lastClickCollider = null;
      }

      private void TryBeginDrag(Collider2D collider)
      {
         IDrag dragComponent = collider.gameObject.GetComponent<IDrag>();
         if (dragComponent == null) return;

         if (dragComponent.CanDrag())
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
         }
         else
         {
            dragComponent.OnDragAttemptFailed();
         }
      }

      private void TryBeginClick(Collider2D collider)
      {
         IClick click = collider.gameObject.GetComponent<IClick>();
         if (click != null)
         {
            _clickingObject = collider.gameObject;
            Debug.Log("[InputManager] Click started");
            return;
         }

         IDrag drag = collider.gameObject.GetComponent<IDrag>();
         if (drag != null && pickUpEnabled)
         {
            TryBeginDrag(collider);
            Debug.Log("[InputManager] Picked up something!");
         }
      }
   }
}