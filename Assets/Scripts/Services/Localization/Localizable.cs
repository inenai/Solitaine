using Common;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Enums
{
    public class Localizable : MonoBehaviour
    {
        [SerializeField] LocalizationKey STR_KEY;

        void Start()
        {
            OnLangSet(God.Settings.CurrentLanguage);
            EventManager.LanguageSet += OnLangSet;
        }

        void OnDestroy()
        {
            EventManager.LanguageSet -= OnLangSet;
        }

        private void OnLangSet(Language newLang)
        {
            string value = LocalizationService.GetLocalizedText(STR_KEY, newLang);
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

            Text textLegacy = GetComponent<Text>();
            if (textLegacy != null)
            {
                textLegacy.text = value;
            }
        }
    }
}