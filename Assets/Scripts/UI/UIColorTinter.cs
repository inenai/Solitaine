using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    public class UIColorTinter : MonoBehaviour
    {
        public void SetColor(SolitaireKind kind)
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.color = Utils.CommonUtils.GetUIImageColor(kind);
            }

            TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = Utils.CommonUtils.GetUITextColor(kind);
            }
        }
    }
}