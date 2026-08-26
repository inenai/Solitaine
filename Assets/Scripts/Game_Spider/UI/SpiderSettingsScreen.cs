using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Spider
{
    public class SpiderSettingsScreen : GameSettingsScreen
    {
        [SerializeField] ToggleGroup _suitsToggleGroup;
        [SerializeField] Toggle _oneSuitToggle;
        [SerializeField] Toggle _twoSuitsToggle;
        [SerializeField] Toggle _fourSuitsToggle;

        protected override void ResetToggles()
        {
            _oneSuitToggle.isOn = false;
            _twoSuitsToggle.isOn = false;
            _fourSuitsToggle.isOn = false;
        }

        protected override void InitToggleGroups()
        {
            Logs.Log("[Settings] Initializing values");
            _suitsToggleGroup.RegisterToggle(_oneSuitToggle);
            _suitsToggleGroup.RegisterToggle(_twoSuitsToggle);
            _suitsToggleGroup.RegisterToggle(_fourSuitsToggle);

            if (SpiderSettings.SuitsAmount == 1)
            {
                _oneSuitToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_oneSuitToggle);
            }

            if (SpiderSettings.SuitsAmount == 2)
            {
                _twoSuitsToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_twoSuitsToggle);
            }

            if (SpiderSettings.SuitsAmount == 4)
            {
                _fourSuitsToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_fourSuitsToggle);
            }
        }

        public override void Save()
        {
            if (_oneSuitToggle.isOn) SpiderSettings.SuitsAmount = 1;
            if (_twoSuitsToggle.isOn) SpiderSettings.SuitsAmount = 2;
            if (_fourSuitsToggle.isOn) SpiderSettings.SuitsAmount = 4;
        }

    }
}