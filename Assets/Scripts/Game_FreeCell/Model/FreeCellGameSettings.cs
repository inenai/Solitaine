using Common;
using UnityEngine;
namespace FreeCell
{
    public static class FreeCellGameSettings
    {

        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(PlayerPrefsKeys.Fc_WinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(PlayerPrefsKeys.Fc_WinCountKey, value);
                PlayerPrefs.Save();
            }
        }

        private static void Log(string message)
        {
            Logs.Log($"[FreeCellSettings] {message}");
        }
    }
}