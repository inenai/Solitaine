using UnityEngine;

namespace Common
{
    public static class GlobalSettings
    {
        static Configs _config;
        static Language _currentLang = Language.ENGLISH;

        public static Configs Config => _config;
        public static Language CurrentLanguage
        {
            get { return _currentLang; }
            set {
                _currentLang = value;
                EventManager.LanguageSet?.Invoke(_currentLang);
            }
        }

        public static void LoadConfig(Configs config)
        {
            if (_config != null) return;
            _config = config;
            CurrentLanguage = GetSavedLang();
        }

        public static void SetCurrentLang(Language lang)
        {
            CurrentLanguage = lang;
            PlayerPrefs.SetString("Lang", CurrentLanguage.ToString());
            PlayerPrefs.Save();
        }

        private static Language GetSavedLang()
        {
            string lang = PlayerPrefs.GetString("Lang", Language.ENGLISH.ToString());

            if (lang == Language.ENGLISH.ToString()) return Language.ENGLISH;
            if (lang == Language.SPANISH.ToString()) return Language.SPANISH;

            return Language.ENGLISH;
        }
    }
}