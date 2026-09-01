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

        protected override void ResetToggles()
        {
            _variantScorpionToggle.isOn = false;
            _variantScorpion2Toggle.isOn = false;
        }

        protected override void InitToggleGroups()
        {
            // Logs.Log("[Settings] Initializing values");
            _variantOption.RegisterToggle(_variantScorpionToggle);
            _variantOption.RegisterToggle(_variantScorpion2Toggle);

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

            _variantOption.RegisterToggle(_variantScorpionToggle);
            _variantOption.RegisterToggle(_variantScorpion2Toggle);
        }

        public override void Save()
        {
            if (_variantScorpionToggle.isOn) ScorpionSettings.Variant = ScorpionVariant.SCORPION;
            if (_variantScorpion2Toggle.isOn) ScorpionSettings.Variant = ScorpionVariant.SCORPION_II;
        }
    }
}