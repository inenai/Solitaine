using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableCardStack : MonoBehaviour, IDrag
{
    public void OnStartDrag()
    {
        Debug.Log("DraggableCardStack OnStartDrag");
    }

    public void OnEndDrag()
    {
        Debug.Log("DraggableCardStack OnEndDrag");
    }

    public bool CanDrag()
    {
        return true;
    }
}
