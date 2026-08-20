using Common;
using UnityEngine;

namespace Spider
{
    public class SpiderSettings : MonoBehaviour
    {
        public const int DEFAULT_SUITS = 1;
        public static bool SavedSettingsAvailable => PlayerPrefs.GetInt(PlayerPrefsKeys.Sp_SavedSettingsFlag, 0) != 0;

        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Sp_WinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Sp_WinCountKey, value);
                PlayerPrefs.Save();
            }
        }

        public static int SuitsAmount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Sp_SuitsAmount, DEFAULT_SUITS);
            }
            set
            {
                Log($"Available restocks set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Sp_SuitsAmount, value);
                Save();
            }
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Sp_SavedSettingsFlag, 1);
            PlayerPrefs.Save();
        }

        private static void Log(string message)
        {
            Debug.Log($"[SpiderSettings] {message}");
        }

        public static void Reset(SpiderConfig defaultConfig)
        {
            PlayerPrefs.SetInt(PlayerPrefsKeys.Sp_SuitsAmount, defaultConfig.Suits);
            Save();
        }
    }
}