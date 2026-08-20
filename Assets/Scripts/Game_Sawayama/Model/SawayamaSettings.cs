using Common;
using UnityEngine;

namespace Sawayama
{
    public static class SawayamaSettings
    {
        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Sw_WinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Sw_WinCountKey, value);
                PlayerPrefs.Save();
            }
        }

        private static void Log(string message)
        {
            Debug.Log($"[SawayamaSettings] {message}");
        }
    }
}