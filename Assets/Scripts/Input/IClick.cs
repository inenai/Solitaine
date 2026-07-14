namespace Common
{
    public interface IClick
    {
        void OnClick();
        bool CanClick();
        void OnClickAttemptFailed();
    }
}