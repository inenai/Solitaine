using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Input
{
   public class MyInputManager : MonoBehaviour
   {
      [Header("Input References")]
      [SerializeField] private InputActionReference pointerMovedAction;
      [SerializeField] private InputActionReference pointerDownAction;
      [SerializeField] private InputActionReference doublePressAction;

      [SerializeField] private float mouseDragSpeed = 0.1f;
      [SerializeField] private float dragDepth = 5f;
      [SerializeField] private Vector3 dragOffset = new Vector3(0, 0.3f, 0);

      private Camera mainCamera;
      private Plane dragPlane;
      private Vector3 velocity = Vector3.zero;
      private GameObject draggingObject;
      private GameObject clickingObject;
      private Vector3 pointerPosition;
      private bool dragging => draggingObject != null;
      private bool clicking => clickingObject != null;

      private void Awake()
      {
         mainCamera = Camera.main;
         dragPlane = new Plane(Vector3.forward, dragDepth);
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
         Debug.Log("[InputManager] Pointer pressed");
         Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            Debug.Log("[InputManager] Collider hit!");
            TryBeginDrag(hit.collider);
            TryBeginClick(hit.collider);
         }
      }

      private void PointerMoved(InputAction.CallbackContext context)
      {
         //Debug.Log("[InputManager] Pointer moved");
         pointerPosition = context.ReadValue<Vector2>();
         if (dragging)
         {
            Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
            if (dragPlane.Raycast(ray, out float distance))
            {
               draggingObject.transform.position = Vector3.SmoothDamp(draggingObject.transform.position, ray.GetPoint(distance), ref velocity, mouseDragSpeed) + dragOffset;
            }
         }
      }

      private void PointerReleased(InputAction.CallbackContext context)
      {
         Debug.Log("[InputManager] Pointer released");

         if (dragging)
         {
            draggingObject.GetComponent<IDrag>()?.OnEndDrag();
            draggingObject = null;
            Debug.Log("[InputManager] Drag ended");
         }

         if (clicking)
         {
            Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider != null && hit.collider.gameObject == clickingObject)
            {
              clickingObject.GetComponent<IClick>()?.OnClick();
            }
            clickingObject = null;
            Debug.Log("[InputManager] Click ended (happened)");
         }
      }

      private void DoublePressed(InputAction.CallbackContext context)
      {
         Debug.Log("[InputManager] Pointer double pressed");
         Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
         RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

         if (hit.collider != null)
         {
            hit.collider.gameObject.GetComponent<IDoubleClick>()?.OnDoubleClick();
         }
      }


      private void TryBeginDrag(Collider2D collider)
      {
         IDrag dragComponent = collider.gameObject.GetComponent<IDrag>();
         if (dragComponent != null && dragComponent.CanDrag())
         {
            draggingObject = collider.gameObject;
            dragComponent.OnStartDrag();
            Debug.Log("[InputManager] Drag started");
         }
      }

      private void TryBeginClick(Collider2D collider)
      {
         IClick click = collider.gameObject.GetComponent<IClick>();
         if (click != null)
         {
            clickingObject = collider.gameObject;
            Debug.Log("[InputManager] Click started");
         }
      }
   }
}