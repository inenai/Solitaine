using UnityEngine;

namespace Common
{
    public class BaseSettingsScreen : MonoBehaviour
    {

        private GameSettingsScreen _screen;

        void Awake()
        {
            _screen = GetComponent<GameSettingsScreen>();
        }

        void OnEnable()
        {
            EventManager.OnMenuOpened?.Invoke();
            _screen.OnEnabled();
        }

        public void SaveAndClose()
        {
            _screen.Save();
            Close();
        }

        public void SaveAndStart()
        {
            SaveAndClose();
            GameNavigator.LoadGame(_screen.Kind);
        }

        public void SaveAndRestart()
        {
            SaveAndClose();
            EventManager.OnResetGameRequested?.Invoke();
        }

        public void Close()
        {
            EventManager.OnMenuClosed?.Invoke();
            gameObject.SetActive(false);
        }
    }
}