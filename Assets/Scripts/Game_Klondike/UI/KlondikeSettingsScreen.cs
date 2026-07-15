using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Klondike
{
    public class KlondikeSettingsScreen : GameSettingsScreen
    {
        [SerializeField] ToggleGroup _drawOption;
        [SerializeField] Toggle _draw1Toggle;
        [SerializeField] Toggle _draw3Toggle;

        [SerializeField] ToggleGroup _restockOption;
        [SerializeField] Toggle _restockInfToggle;
        [SerializeField] Toggle _restock3Toggle;

        public override void OnEnabled()
        {
            ResetToggles();
            InitToggleGroups();
        }

        private void ResetToggles()
        {
            _draw1Toggle.isOn = false;
            _draw3Toggle.isOn = false;
            _restock3Toggle.isOn = false;
            _restockInfToggle.isOn = false;
        }

        private void InitToggleGroups()
        {
            Debug.Log("[Settings] Initializing values");
            _drawOption.RegisterToggle(_draw1Toggle);
            _drawOption.RegisterToggle(_draw3Toggle);

            if (KlondikeSettings.DrawAmount == 1)
            {
                _draw1Toggle.isOn = true;
                _drawOption.NotifyToggleOn(_draw1Toggle);
                Debug.Log("[Settings] Draw amount is 1, turn on toggle for \"Draw: One\"");
            }
            if (KlondikeSettings.DrawAmount == 3)
            {
                _draw3Toggle.isOn = true;
                _drawOption.NotifyToggleOn(_draw3Toggle);
                Debug.Log("[Settings] Draw amount is 3, turn on toggle for \"Draw: Three\"");
            }

            _restockOption.RegisterToggle(_restock3Toggle);
            _restockOption.RegisterToggle(_restockInfToggle);

            if (KlondikeSettings.AvailableRestocks == 3)
            {
                _restock3Toggle.isOn = true;
                _restockOption.NotifyToggleOn(_restock3Toggle);
                Debug.Log("[Settings] Available Restocks is 3, turn on toggle for \"Restock: Three\"");
            }
            if (KlondikeSettings.AvailableRestocks == -1)
            {
                _restockInfToggle.isOn = true;
                _restockOption.NotifyToggleOn(_restockInfToggle);
                Debug.Log("[Settings] Available Restocks is -1, turn on toggle for \"Restock: Unlimited\"");
            }
        }

        public override void Save()
        {
            if (_draw1Toggle.isOn) KlondikeSettings.DrawAmount = 1;
            if (_draw3Toggle.isOn) KlondikeSettings.DrawAmount = 3;

            if (_restock3Toggle.isOn) KlondikeSettings.AvailableRestocks = 3;
            if (_restockInfToggle.isOn) KlondikeSettings.AvailableRestocks = -1;
        }
    }
}