using UnityEngine;

namespace Sawayama
{
    public static class SawayamaSettings
    {
        const string SawayamaWinCountKey = "SW_WIN_COUNT";
        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(SawayamaWinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(SawayamaWinCountKey, value);
                PlayerPrefs.Save();
            }
        }

        private static void Log(string message)
        {
            Debug.Log($"[SawayamaSettings] {message}");
        }
    }
}