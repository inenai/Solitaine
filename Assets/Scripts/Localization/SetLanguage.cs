using UnityEngine;

namespace Common {
    public class SetLanguage : MonoBehaviour
    {
        public void SetLanguageToEnglish()
        {
            GlobalSettings.SetCurrentLang(Language.ENGLISH);
        }

        public void SetLanguageToSpanish()
        {
            GlobalSettings.SetCurrentLang(Language.SPANISH);
        }
    }
}