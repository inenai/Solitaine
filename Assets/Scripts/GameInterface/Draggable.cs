using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Draggable : MonoBehaviour, IDrag
{
    private Vector3 _positionOnStartDrag;
    private Collider2D _collider;

    void Awake()
    {
        _positionOnStartDrag = transform.position;
        _collider = GetComponent<Collider2D>();
    }

    public bool CanDrag()
    {
        return true;
    }

    public void OnStartDrag()
    {
        Debug.Log("DraggableCardStack OnStartDrag");
        _positionOnStartDrag = transform.position;
    }

    public void OnEndDrag()
    {
        Debug.Log("DraggableCardStack OnEndDrag");
        //TODO if drag fails, reset position with
        // transform.position = positionOnStartDrag;
    }

    public void EnableDrag(bool value)
    {
        _collider.enabled = value;
    }
}
