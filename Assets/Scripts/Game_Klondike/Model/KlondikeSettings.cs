using UnityEngine;

namespace Klondike
{
    public static class KlondikeSettings
    {
        const string SavedSettingsFlag = "KL_SAVED_SETTINGS";
        const string DrawAmountKey = "KL_DRAW_AMOUNT";
        const string AllowRedrawKey = "KL_ALLOW_REDRAW";
        const string FoundationCardsFreeKey = "KL_FOUNDATION_CARDS_FREE";
        public const int DEFAULT_RESTOCKS = -1;
        public const int DEFAULT_DRAW_AMOUNT = 3;
        public const bool DEFAULT_FOUNDATION_CARDS_FREE = false;

        public static bool SavedSettingsAvailable => PlayerPrefs.GetInt(SavedSettingsFlag, 0) != 0;

        public static int DrawAmount
        {
            get
            {
                return PlayerPrefs.GetInt(DrawAmountKey, DEFAULT_DRAW_AMOUNT);
            }
            set
            {
                PlayerPrefs.SetInt(DrawAmountKey, value);
                Save();
            }
        }

        public static int AvailableRestocks
        {
            get
            {
                return PlayerPrefs.GetInt(AllowRedrawKey, DEFAULT_RESTOCKS);
            }
            set
            {
                PlayerPrefs.SetInt(AllowRedrawKey, value);
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
                PlayerPrefs.SetInt(AllowRedrawKey, value ? 1 : 0);
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
            DrawAmount = defaultConfig.DrawAmount;
            AvailableRestocks = defaultConfig.AvailableRestocks;
            FoundationCardsFree = defaultConfig.FoundationCardsFree;
        }
    }
}