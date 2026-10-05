namespace Common
{
    public interface IDrag
    {
        void OnStartDrag();
        void OnEndDrag(bool cancelled);
        bool CanDrag();
        void OnDragAttemptFailed();
    }
}