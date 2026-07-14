using UnityEngine;
using UnityEngine.UI;

namespace Klondike
{
    public class KlondikeSettingsScreen : MonoBehaviour
    {
        [SerializeField] KlondikeUI _kUI;
        [SerializeField] ToggleGroup _drawOption;
        [SerializeField] Toggle _draw1Toggle;
        [SerializeField] Toggle _draw3Toggle;

        [SerializeField] ToggleGroup _restockOption;
        [SerializeField] Toggle _restock3Toggle;
        [SerializeField] Toggle _restockInfToggle;

        void OnEnable()
        {
            InitToggleGroups();
        }

        private void InitToggleGroups()
        {
            _drawOption.RegisterToggle(_draw1Toggle);
            _drawOption.RegisterToggle(_draw3Toggle);

            if (KlondikeSettings.DrawAmount == 1) _drawOption.NotifyToggleOn(_draw1Toggle);
            if (KlondikeSettings.DrawAmount == 3) _drawOption.NotifyToggleOn(_draw3Toggle);

            _restockOption.RegisterToggle(_restock3Toggle);
            _restockOption.RegisterToggle(_restockInfToggle);

            if (KlondikeSettings.AvailableRestocks == 3) _restockOption.NotifyToggleOn(_restock3Toggle);
            if (KlondikeSettings.AvailableRestocks == -1) _restockOption.NotifyToggleOn(_restockInfToggle);
        }

        private void Save()
        {
            if (_draw1Toggle.isOn) KlondikeSettings.DrawAmount = 1;
            if (_draw3Toggle.isOn) KlondikeSettings.DrawAmount = 3;

            if (_restock3Toggle.isOn) KlondikeSettings.AvailableRestocks = 3;
            if (_restockInfToggle.isOn) KlondikeSettings.AvailableRestocks = -1;
        }

        public void SaveAndClose()
        {
            Save();
            _kUI.CloseSettings();
        }

        public void SaveAndRestart()
        {
            SaveAndClose();
            _kUI.RestartGame();
        }
    }
}