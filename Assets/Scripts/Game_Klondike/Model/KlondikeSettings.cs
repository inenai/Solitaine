using UnityEngine;

namespace Klondike
{
    public static class KlondikeSettings
    {
        const string SavedSettingsFlag = "KL_SAVED_SETTINGS";
        const string DrawAmountKey = "KL_DRAW_AMOUNT";
        const string AvailableRestocksKey = "KL_AVAILABLE_RESTOCKS";
        const string FoundationCardsFreeKey = "KL_FOUNDATION_CARDS_FREE";
        const string AutoMovesEnabledKey = "KL_AUTO_MOVES_ENABLED";
        const string KlondikeWinCountKey = "KL_WIN_COUNT";

        public const int DEFAULT_RESTOCKS = -1;
        public const int DEFAULT_DRAW_AMOUNT = 3;
        public const bool DEFAULT_FOUNDATION_CARDS_FREE = false;
        public const bool DEFAULT_AUTO_MOVES_ENABLED = true;

        public static bool SavedSettingsAvailable => PlayerPrefs.GetInt(SavedSettingsFlag, 0) != 0;

        public static int DrawAmount
        {
            get
            {
                return PlayerPrefs.GetInt(DrawAmountKey, DEFAULT_DRAW_AMOUNT);
            }
            set
            {
                Log($"Draw amount set to {value}");
                PlayerPrefs.SetInt(DrawAmountKey, value);
                Save();
            }
        }

        public static int AvailableRestocks
        {
            get
            {
                return PlayerPrefs.GetInt(AvailableRestocksKey, DEFAULT_RESTOCKS);
            }
            set
            {
                Log($"Available restocks set to {value}");
                PlayerPrefs.SetInt(AvailableRestocksKey, value);
                Save();
            }
        }

        public static bool FoundationCardsFree
        {
            get
            {
                return PlayerPrefs.GetInt(FoundationCardsFreeKey, DEFAULT_FOUNDATION_CARDS_FREE ? 1 : 0) != 0;
            }
            set
            {
                Log($"Free foundations set to {value}");
                PlayerPrefs.SetInt(AvailableRestocksKey, value ? 1 : 0);
                Save();
            }
        }

        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(KlondikeWinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(KlondikeWinCountKey, value);
                Save();
            }
        }

        public static bool AutoMovesEnabled
        {
            get
            {
                return PlayerPrefs.GetInt(AutoMovesEnabledKey, DEFAULT_AUTO_MOVES_ENABLED ? 1 : 0) != 0;
            }
            set
            {
                Log($"Auto moves enabled set to {value}");
                PlayerPrefs.SetInt(AutoMovesEnabledKey, value ? 1 : 0);
                Save();
            }
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(SavedSettingsFlag, 1);
            PlayerPrefs.Save();
        }

        public static void Reset(KlondikeConfig defaultConfig)
        {
            PlayerPrefs.SetInt(DrawAmountKey, defaultConfig.DrawAmount);
            PlayerPrefs.SetInt(AvailableRestocksKey, defaultConfig.AvailableRestocks);
            PlayerPrefs.SetInt(FoundationCardsFreeKey, defaultConfig.FoundationCardsFree ? 1 : 0);
            PlayerPrefs.SetInt(AutoMovesEnabledKey, defaultConfig.AutoMovesEnabled ? 1 : 0);
            Save();
        }

        private static void Log(string message)
        {
            Debug.Log($"[KlondikeSettings] {message}");
        }
    }
}