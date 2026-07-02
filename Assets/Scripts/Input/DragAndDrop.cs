using UnityEngine;
using UnityEngine.InputSystem;

public class DragAndDrop : MonoBehaviour
{
   [Header("Input References")]
   [SerializeField] private InputActionReference pointerMovedAction;
   [SerializeField] private InputActionReference pointerDownAction;

   [SerializeField] private float mouseDragSpeed = 0.1f;
   [SerializeField] private float dragDepth = 0.5f;
   [SerializeField] private Vector3 dragOffset = new Vector3(0, 0.3f, 0);

   private Camera mainCamera;
   private Plane dragPlane;
   private Vector3 velocity = Vector3.zero;
   private GameObject draggedObject;
   private Vector3 pointerPosition;
   private bool dragging => draggedObject != null;

   private void Awake()
   {
      mainCamera = Camera.main;
      dragPlane = new Plane(Vector3.forward, dragDepth);
   }

   private void OnEnable()
   {
      pointerMovedAction.action.Enable();
      pointerDownAction.action.Enable();

      pointerDownAction.action.performed += PointerPressed;
      pointerMovedAction.action.performed += PointerMoved;
      pointerDownAction.action.canceled += PointerReleased;
   }

   private void OnDisable()
   {
      pointerMovedAction.action.Disable();
      pointerDownAction.action.Disable();

      pointerDownAction.action.performed -= PointerPressed;
      pointerMovedAction.action.performed -= PointerMoved;
      pointerDownAction.action.canceled -= PointerReleased;
   }

   private void PointerPressed(InputAction.CallbackContext context)
   {
      Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
      RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

      if (hit.collider != null)
      {
         IDrag dragComponent = hit.collider.gameObject.GetComponent<IDrag>();

         if (dragComponent != null && dragComponent.CanDrag())
         {
            draggedObject = hit.collider.gameObject;
            draggedObject.TryGetComponent<IDrag>(out var iDragComponent);
            iDragComponent?.OnStartDrag();
         }
      }
   }

   private void PointerMoved(InputAction.CallbackContext context)
   {
      pointerPosition = context.ReadValue<Vector2>();
      if (dragging)
      {
         Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
         if (dragPlane.Raycast(ray, out float distance))
         {
            draggedObject.transform.position = Vector3.SmoothDamp(draggedObject.transform.position, ray.GetPoint(distance), ref velocity, mouseDragSpeed) + dragOffset;
         }
      }
   }

   private void PointerReleased(InputAction.CallbackContext context)
   {
      if (dragging)
      {
         draggedObject.TryGetComponent<IDrag>(out var iDragComponent);
         iDragComponent?.OnEndDrag();
         draggedObject = null;
      }
   }
}
