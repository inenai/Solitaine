using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace Services
{
   public class InputService : Service
   {
      [Header("Settings")]
      [SerializeField] private float _peekHoldDelay = 0.3f;
      [Header("Input References")]
      [SerializeField] private InputActionReference pointerMovedAction;
      [SerializeField] private InputActionReference pointerDownAction;
      [SerializeField] private InputActionReference doublePressAction;
      [SerializeField] private InputActionReference resetGameAction;
      [SerializeField] private InputActionReference drawFromStockAction;
      [SerializeField] private InputActionReference cancelDrag;
      [SerializeField] private InputActionReference undo;
      [SerializeField] private InputActionReference redo;
      [SerializeField] private InputActionReference peek;

      [SerializeField] private float mouseDragSpeed = 0.1f;

      public float DragDepth = 9f;

      private bool InputBlocked
      {
         get
         {
            //Logs.Log($"_inputBlockers.Count: {_inputBlockers.Count}");
            return _inputBlockers.Count > 0;
         }
      }
      private HashSet<string> _inputBlockers;
      private Vector3 _dragOffset;
      private Camera _mainCamera;
      private Plane _dragPlane;
      private Vector3 _velocity = Vector3.zero;
      private GameObject _draggingObject;
      private GameObject _clickingObject;
      private IPeek _peekingObject;
      private Vector3 _pointerPosition;
      private bool dragging => _draggingObject != null;
      private bool clicking => _clickingObject != null;
      private bool peeking => _peekingObject != null;
      private Collider2D _previousClickedCollider;
      private Collider2D _lastClickCollider;
      private Coroutine _longHoldPeekCR;

      public override void Init()
      {
         _inputBlockers = new();
         _mainCamera = Camera.main;
         _dragPlane = new Plane(Vector3.forward, DragDepth);
      }

      private void OnEnable()
      {
         EnhancedTouchSupport.Enable();

         // Subscribe to explicit EnhancedTouch events to avoid bug when switching from pen to finger
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += OnFingerDown;
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove += OnFingerMove;
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp += OnFingerUp;

         pointerMovedAction.action.Enable();
         pointerDownAction.action.Enable();
         resetGameAction.action.Enable();
         drawFromStockAction.action.Enable();

         undo.action.Enable();
         redo.action.Enable();
         peek.action.Enable();

         pointerDownAction.action.performed += Action_PointerPressed;
         pointerMovedAction.action.performed += Action_PointerMoved;
         pointerDownAction.action.canceled += Action_PointerReleased;
         resetGameAction.action.performed += Action_ResetGame;
         drawFromStockAction.action.performed += Action_DrawFromStock;
         cancelDrag.action.performed += Action_CancelDrag;
         undo.action.performed += Action_Undo;
         redo.action.performed += Action_Redo;
         peek.action.performed += Action_StartPeek;
         peek.action.canceled += Action_EndPeek;
         doublePressAction.action.Enable();
         doublePressAction.action.performed += Action_DoublePressed;
      }

      private void OnDisable()
      {
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= OnFingerDown;
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerMove -= OnFingerMove;
         UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerUp -= OnFingerUp;

         pointerMovedAction.action.Disable();
         pointerDownAction.action.Disable();
         resetGameAction.action.Disable();
         drawFromStockAction.action.Disable();

         undo.action.Disable();
         redo.action.Disable();
         peek.action.Disable();

         pointerDownAction.action.performed -= Action_PointerPressed;
         pointerMovedAction.action.performed -= Action_PointerMoved;
         pointerDownAction.action.canceled -= Action_PointerReleased;
         resetGameAction.action.performed -= Action_ResetGame;
         drawFromStockAction.action.performed -= Action_DrawFromStock;
         cancelDrag.action.performed -= Action_CancelDrag;
         undo.action.performed -= Action_Undo;
         redo.action.performed -= Action_Redo;
         peek.action.performed -= Action_StartPeek;
         peek.action.canceled -= Action_EndPeek;
         doublePressAction.action.Disable();
         doublePressAction.action.performed -= Action_DoublePressed;
      }

      #region EnhancedTouch
      private void OnFingerDown(Finger finger)
      {
         // Logs.Log("[InputManager] On finger down");
         if (InputBlocked) return;

         _pointerPosition = finger.currentTouch.screenPosition;

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            _previousClickedCollider = _lastClickCollider;
            _lastClickCollider = hit.collider;

            TryBeginDrag(hit.collider);
            TryBeginClick(hit.collider);
            // Logs.Log("[InputManager] On finger down: start delayed peek");
            _longHoldPeekCR = StartCoroutine(StartDelayedPeek(hit.collider));
         }
      }

      private void OnFingerMove(Finger finger)
      {
         if (InputBlocked) return;

         _pointerPosition = finger.currentTouch.screenPosition;

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

      private void OnFingerUp(Finger finger)
      {
         // Logs.Log("[InputManager] On finger up");
         if (InputBlocked)
         {
            // Logs.Log("[InputManager] On finger up: Input is blocked, we end drag and peek");
            EndDrag(cancelled: true);
            TryEndPeek();
            _clickingObject = null;
            return;
         }

         if (dragging)
         {
            EndDrag(cancelled: false);
         }

         TryEndPeek();

         if (clicking)
         {
            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null)
            {
               IClick c = hit.collider.gameObject.GetComponent<IClick>();
               if (c != null && hit.collider.gameObject == _clickingObject)
               {
                  if (c.CanClick()) c.OnClick();
                  else c.OnClickAttemptFailed();
               }
            }
            _clickingObject = null;
         }
      }
      #endregion

      public void BlockInput(string reason)
      {
         if (_inputBlockers.Contains(reason))
         {
            Debug.LogError($"Input block reason already used: {reason}");
            return;
         }
         //Logs.Log($"Input block reason added: {reason}");
         _inputBlockers.Add(reason);
      }

      public void UnblockInput(string reason)
      {
         if (!_inputBlockers.Contains(reason))
         {
            Debug.LogError($"Input block reason not used: {reason}");
            return;
         }
         //Logs.Log($"Input block reason removed: {reason}");
         _inputBlockers.Remove(reason);
      }

      private void Action_ResetGame(InputAction.CallbackContext context)
      {
         if (dragging) return;
         EventManager.OnResetGameRequested?.Invoke();
      }

      private void Action_DrawFromStock(InputAction.CallbackContext context)
      {
         if (dragging) return;
         EventManager.OnDrawFromStock?.Invoke();
      }

      private void Action_PointerPressed(InputAction.CallbackContext context)
      {
         // Logs.Log("[InputManager] Pointer pressed");
         if (InputBlocked) return;
         _pointerPosition = pointerMovedAction.action.ReadValue<Vector2>();

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            // Logs.Log("[InputManager] Collider hit!");
            _previousClickedCollider = _lastClickCollider;
            _lastClickCollider = hit.collider;

            TryBeginDrag(hit.collider);
            TryBeginClick(hit.collider);
         }
      }

      private void Action_CancelDrag(InputAction.CallbackContext context)
      {
         EndDrag(cancelled: true);
      }

      private void Action_Undo(InputAction.CallbackContext context)
      {
         EventManager.OnUndo?.Invoke();
      }

      private void Action_Redo(InputAction.CallbackContext context)
      {
         EventManager.OnRedo?.Invoke();
      }

      private void Action_StartPeek(InputAction.CallbackContext context)
      {
         if (dragging) return;
         if (InputBlocked) return;
         _pointerPosition = pointerMovedAction.action.ReadValue<Vector2>();

         Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            TryBeginPeek(hit.collider);
         }
      }

      private void Action_EndPeek(InputAction.CallbackContext context)
      {
         // Logs.Log($"[InputManager] Action_EndPeek");
         TryEndPeek();
      }

      private void TryBeginPeek(Collider2D collider)
      {
         if (InputBlocked) return;
         IPeek peekable = collider.gameObject.GetComponent<IPeek>();
         if (peekable != null)
         {
            if (peekable.StartPeeking())
            {
               if (_peekingObject != null && _peekingObject != peekable)
               {
                  _peekingObject.StopPeeking();
               }
               _peekingObject = peekable;
            }
         }
      }

      private IEnumerator StartDelayedPeek(Collider2D collider)
      {
         yield return new WaitForSecondsRealtime(_peekHoldDelay);
         // Logs.Log("[InputManager] On finger down: start delayed peek - Ready to start peek on collider.");
         TryBeginPeek(collider);
         _longHoldPeekCR = null;
      }

      private void TryEndPeek()
      {
         if (_longHoldPeekCR != null)
         {
            StopCoroutine(_longHoldPeekCR);
         }

         if (peeking)
         {
            _peekingObject.StopPeeking();
            _peekingObject = null;
         }
      }

      private void EndDrag(bool cancelled)
      {
         if (!dragging)
            return;

         _draggingObject.GetComponent<IDrag>()?.OnEndDrag(cancelled);
         _draggingObject = null;
         _dragOffset = Vector3.zero;
         _velocity = Vector3.zero;
         cancelDrag.action.Disable();
         // Logs.Log($"[InputManager] Drag ended. Cancelled: {cancelled}");
      }

      private void Action_PointerMoved(InputAction.CallbackContext context)
      {
         //Logs.Log("[InputManager] Pointer moved");
         if (InputBlocked) return;

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
               Logs.Log($"[InputManager] Pointer moved to {ray.GetPoint(distance)}");
            }
         }
      }

      private void Action_PointerReleased(InputAction.CallbackContext context)
      {
         // Logs.Log("[InputManager] Pointer released");
         if (InputBlocked)
         {
            EndDrag(cancelled: true);
            _clickingObject = null;
            return;
         }

         if (dragging)
         {
            EndDrag(cancelled: false);
            // Logs.Log("[InputManager] Drag ended");
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
               }
            }
            _clickingObject = null;
            // Logs.Log("[InputManager] Click ended (happened)");
         }
      }

      private void Action_DoublePressed(InputAction.CallbackContext context)
      {
         // Logs.Log("[InputManager] Pointer double pressed");
         if (InputBlocked) return;

         EndDrag(cancelled: true);

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
            if (_peekingObject != null)
               _peekingObject.StopPeeking();

            if (_draggingObject != null)
               EndDrag(cancelled: true);

            Ray ray = _mainCamera.ScreenPointToRay(_pointerPosition);
            if (_dragPlane.Raycast(ray, out float distance))
            {
               Vector3 fullOffset = collider.transform.position - ray.GetPoint(distance);
               _dragOffset = new Vector3(fullOffset.x, fullOffset.y, 0f);
            }
            _draggingObject = collider.gameObject;
            dragComponent.OnStartDrag();
            cancelDrag.action.Enable();
            // Logs.Log("[InputManager] Drag started");
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
            // Logs.Log("[InputManager] Click started");
            return;
         }
      }
   }
}