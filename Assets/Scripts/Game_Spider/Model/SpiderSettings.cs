using Common;
using UnityEngine;

namespace Spider
{
    public class SpiderSettings : MonoBehaviour
    {
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

        private static void Log(string message)
        {
            Debug.Log($"[SpiderSettings] {message}");
        }
    }
}