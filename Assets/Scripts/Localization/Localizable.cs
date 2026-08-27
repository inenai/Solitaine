using TMPro;
using UnityEngine;

namespace Common
{
    public class Localizable : MonoBehaviour
    {
        [SerializeField] LocalizationKey STR_KEY;

        void Start()
        {
            OnLangSet(GlobalSettings.CurrentLanguage);
            EventManager.LanguageSet += OnLangSet;
        }

        void OnDestroy()
        {
            EventManager.LanguageSet -= OnLangSet;
        }

        private void OnLangSet(Language newLang)
        {
            string value = LocalizationManager.GetLocalizedText(STR_KEY, newLang);
            TextMeshProUGUI textUI = GetComponent<TextMeshProUGUI>();
            if (textUI != null)
            {
                textUI.text = value;
            }

            TextMeshPro text = GetComponent<TextMeshPro>();
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}