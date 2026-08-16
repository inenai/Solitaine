using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(GameNavigator))]
    public class BaseSettingsScreen : MonoBehaviour
    {
        private GameNavigator _navi;
        private GameSettingsScreen _screen;

        void Awake()
        {
            _screen = GetComponent<GameSettingsScreen>();
            _navi = GetComponent<GameNavigator>();
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
            _navi.LoadGame(_screen.Kind);
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