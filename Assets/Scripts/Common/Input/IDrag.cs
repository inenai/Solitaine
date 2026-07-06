namespace Common
{
    public interface IDrag
    {
        void OnStartDrag();
        void OnEndDrag();
        bool CanDrag();
    }
}