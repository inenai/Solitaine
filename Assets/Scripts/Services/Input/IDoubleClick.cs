namespace Common
{
    public interface IDoubleClick
    {
        void OnDoubleClick();
        bool CanDoubleClick();
        void OnDoubleClickAttemptFailed();
    }
}