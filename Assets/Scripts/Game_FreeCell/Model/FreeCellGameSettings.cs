using UnityEngine;
namespace FreeCell
{
    public static class FreeCellGameSettings
    {
        const string FreeCellWinCountKey = "FC_WIN_COUNT";
        public static int WinCount
        {
            get
            {
                return PlayerPrefs.GetInt(FreeCellWinCountKey, 0);
            }
            set
            {
                Log($"Win count set to {value}");
                PlayerPrefs.SetInt(FreeCellWinCountKey, value);
                PlayerPrefs.Save();
            }
        }

        private static void Log(string message)
        {
            Debug.Log($"[FreeCellSettings] {message}");
        }
    }
}