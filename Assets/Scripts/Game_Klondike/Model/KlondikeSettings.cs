using Common;
using UnityEngine;

namespace Klondike
{
    public static class KlondikeSettings
    {
        public const int DEFAULT_RESTOCKS = -1;
        public const int DEFAULT_DRAW_AMOUNT = 3;
        public const bool DEFAULT_FOUNDATION_CARDS_FREE = false;
        public const bool DEFAULT_AUTO_MOVES_ENABLED = true;

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

        public static bool FoundationCardsFree
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_FoundationCardsFreeKey, DEFAULT_FOUNDATION_CARDS_FREE ? 1 : 0) != 0;
            }
            set
            {
                Log($"Free foundations set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_AvailableRestocksKey, value ? 1 : 0);
                Save();
            }
        }

        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_WinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_WinCountKey, value);
                Save();
            }
        }

        public static bool AutoMovesEnabled
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Kl_AutoMovesEnabledKey, DEFAULT_AUTO_MOVES_ENABLED ? 1 : 0) != 0;
            }
            set
            {
                Log($"Auto moves enabled set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_AutoMovesEnabledKey, value ? 1 : 0);
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
            PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_FoundationCardsFreeKey, defaultConfig.FoundationCardsFree ? 1 : 0);
            PlayerPrefs.SetInt(PlayerPrefsKeys.Kl_AutoMovesEnabledKey, defaultConfig.AutoMovesEnabled ? 1 : 0);
            Save();
        }

        private static void Log(string message)
        {
            Debug.Log($"[KlondikeSettings] {message}");
        }
    }
}