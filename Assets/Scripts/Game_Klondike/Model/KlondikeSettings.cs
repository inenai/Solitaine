using Common;
using UnityEngine;

namespace Klondike
{
    public static class KlondikeSettings
    {
        public const int DEFAULT_RESTOCKS = -1;
        public const int DEFAULT_DRAW_AMOUNT = 3;

        public static bool SavedSettingsAvailable => PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_SavedSettingsFlag, 0) != 0;

        public static int DrawAmount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_DrawAmountKey, DEFAULT_DRAW_AMOUNT);
            }
            set
            {
                Log($"Draw amount set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_DrawAmountKey, value);
                Save();
            }
        }

        public static int AvailableRestocks
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_AvailableRestocksKey, DEFAULT_RESTOCKS);
            }
            set
            {
                Log($"Available restocks set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_AvailableRestocksKey, value);
                Save();
            }
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_SavedSettingsFlag, 1);
            PlayerPrefs.Save();
        }

        public static void Reset(KlondikeConfig defaultConfig)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_DrawAmountKey, defaultConfig.DrawAmount);
            PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_AvailableRestocksKey, defaultConfig.AvailableRestocks);
            Save();
        }

        private static void Log(string message)
        {
            Logs.Log($"[KlondikeSettings] {message}");
        }
    }
}