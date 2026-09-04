using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Scorpion
{
    public class ScorpionSettingsScreen : GameSettingsScreen
    {
        [SerializeField] ToggleGroup _variantOption;
        [SerializeField] Toggle _variantScorpionToggle;
        [SerializeField] Toggle _variantScorpion2Toggle;

        [SerializeField] ToggleGroup _suitsToggleGroup;
        [SerializeField] Toggle _oneSuitToggle;
        [SerializeField] Toggle _twoSuitsToggle;
        [SerializeField] Toggle _fourSuitsToggle;

        protected override void ResetToggles()
        {
            _variantScorpionToggle.isOn = false;
            _variantScorpion2Toggle.isOn = false;

            _oneSuitToggle.isOn = false;
            _twoSuitsToggle.isOn = false;
            _fourSuitsToggle.isOn = false;
        }

        protected override void InitToggleGroups()
        {
            // Logs.Log("[Settings] Initializing values");
            _variantOption.RegisterToggle(_variantScorpionToggle);
            _variantOption.RegisterToggle(_variantScorpion2Toggle);

            _suitsToggleGroup.RegisterToggle(_oneSuitToggle);
            _suitsToggleGroup.RegisterToggle(_twoSuitsToggle);
            _suitsToggleGroup.RegisterToggle(_fourSuitsToggle);

            if (ScorpionSettings.Variant == ScorpionVariant.SCORPION)
            {
                _variantScorpionToggle.isOn = true;
                _variantOption.NotifyToggleOn(_variantScorpionToggle);
            }
            if (ScorpionSettings.Variant == ScorpionVariant.SCORPION_II)
            {
                _variantScorpion2Toggle.isOn = true;
                _variantOption.NotifyToggleOn(_variantScorpion2Toggle);
            }

            if (ScorpionSettings.SuitsAmount == 1)
            {
                _oneSuitToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_oneSuitToggle);
            }

            if (ScorpionSettings.SuitsAmount == 2)
            {
                _twoSuitsToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_twoSuitsToggle);
            }

            if (ScorpionSettings.SuitsAmount == 4)
            {
                _fourSuitsToggle.isOn = true;
                _suitsToggleGroup.NotifyToggleOn(_fourSuitsToggle);
            }
        }

        public override void Save()
        {
            if (_variantScorpionToggle.isOn) ScorpionSettings.Variant = ScorpionVariant.SCORPION;
            if (_variantScorpion2Toggle.isOn) ScorpionSettings.Variant = ScorpionVariant.SCORPION_II;

            if (_oneSuitToggle.isOn) ScorpionSettings.SuitsAmount = 1;
            if (_twoSuitsToggle.isOn) ScorpionSettings.SuitsAmount = 2;
            if (_fourSuitsToggle.isOn) ScorpionSettings.SuitsAmount = 4;
        }
    }
}