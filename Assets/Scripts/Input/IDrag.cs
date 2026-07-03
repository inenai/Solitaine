namespace Common.Input
{
    public interface IDrag
    {
        void OnStartDrag();
        void OnEndDrag();
        bool CanDrag();
    }
}