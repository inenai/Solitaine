using Common;
using UnityEngine;

namespace Scorpion
{
    public enum ScorpionVariant
    {
        SCORPION,
        SCORPION_II,
    }

    public static class ScorpionSettings
    {
        public const ScorpionVariant DEFAULT_VARIANT = ScorpionVariant.SCORPION;
        public static bool SavedSettingsAvailable => PlayerPrefs.GetInt(PlayerPrefsKeys.Sc_SavedSettingsFlag, 0) != 0;

        public static ScorpionVariant Variant
        {
            get
            {
                return (ScorpionVariant)PlayerPrefs.GetInt(PlayerPrefsKeys.Sc_Variant, 0);
            }
            set
            {
                Log($"Scorpion variant set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Sc_Variant, (int)value);
                Save();
            }
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Sc_SavedSettingsFlag, 1);
            PlayerPrefs.Save();
        }

        public static void Reset(ScorpionConfig defaultConfig)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Sc_Variant, (int)defaultConfig.Variant);
            Save();
        }

        private static void Log(string message)
        {
            Logs.Log($"[ScorpionSettings] {message}");
        }
    }
}