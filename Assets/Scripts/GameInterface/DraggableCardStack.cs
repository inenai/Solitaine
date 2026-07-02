using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableCardStack : MonoBehaviour, IDrag
{
    private Vector3 positionOnStartDrag;

    void Awake()
    {
        positionOnStartDrag = transform.position;
    }

    public bool CanDrag()
    {
        return true;
    }

    public void OnStartDrag()
    {
        Debug.Log("DraggableCardStack OnStartDrag");
        positionOnStartDrag = transform.position;
    }

    public void OnEndDrag()
    {
        Debug.Log("DraggableCardStack OnEndDrag");
        //TODO if drag fails, reset position with
        // transform.position = positionOnStartDrag;
    }
}
