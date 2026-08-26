using UnityEngine;

namespace Common
{
    public abstract class GameSettingsScreen : MonoBehaviour, ISolitaireVariant
    {
        protected const string MENU_ID_PREFIX = "MENU_Settings_";
        protected SolitaireKind _kind;

        public abstract void Save();
        protected abstract void ResetToggles();
        protected abstract void InitToggleGroups();

        private void OnEnable()
        {
            EventManager.OnMenuOpened?.Invoke(MENU_ID_PREFIX + _kind.ToString());
            ResetToggles();
            InitToggleGroups();
        }

        public void Configure(SolitaireKind kind)
        {
            _kind = kind;
        }

        public void SaveAndClose()
        {
            Save();
            Close();
        }

        public void SaveAndStart()
        {
            SaveAndClose();
            GameNavigator.LoadGame(_kind);
        }

        public void SaveAndRestart()
        {
            SaveAndClose();
            EventManager.OnResetGameRequested?.Invoke();
        }

        public void Close()
        {
            EventManager.OnMenuClosed?.Invoke(MENU_ID_PREFIX + _kind.ToString());
            gameObject.SetActive(false);
        }
    }
}