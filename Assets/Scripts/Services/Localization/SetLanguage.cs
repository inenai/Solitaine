using Common;
using Services;
using UnityEngine;

namespace Enums {
    public class SetLanguage : MonoBehaviour
    {
        public void SetLanguageToEnglish()
        {
            God.Settings.SetCurrentLang(Language.ENGLISH);
        }

        public void SetLanguageToSpanish()
        {
            God.Settings.SetCurrentLang(Language.SPANISH);
        }
    }
}