using Common;
using UnityEngine;

namespace Services
{
    public class SettingsService : Service
    {
        [SerializeField] Settings _config;
        Language _currentLang = Language.ENGLISH;
        bool _autoMoves = true;

        public Settings Config => _config;
        public Language CurrentLanguage
        {
            get { return _currentLang; }
            set
            {
                _currentLang = value;
                EventManager.LanguageSet?.Invoke(_currentLang);
            }
        }

        public bool AutoMoves => _autoMoves;

        public override void Init()
        {
            CurrentLanguage = GetSavedLang();
        }

        public void SetCurrentLang(Language lang)
        {
            CurrentLanguage = lang;
            PlayerPrefs.SetString(PlayerPrefsKeys.GS_Language, CurrentLanguage.ToString());
            PlayerPrefs.Save();
        }

        private Language GetSavedLang()
        {
            string lang = PlayerPrefs.GetString(PlayerPrefsKeys.GS_Language, Language.ENGLISH.ToString());

            if (lang == Language.ENGLISH.ToString()) return Language.ENGLISH;
            if (lang == Language.SPANISH.ToString()) return Language.SPANISH;

            return Language.ENGLISH;
        }

        public bool GetAutoMovesEnabled()
        {
            return PlayerPrefs.GetInt(PlayerPrefsKeys.GS_AutoMoves, 1) != 0;
        }

        public void SetAutoMovesEnabled(bool enabled)
        {
            _autoMoves = enabled;
            PlayerPrefs.SetInt(PlayerPrefsKeys.GS_AutoMoves, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }


    }
}