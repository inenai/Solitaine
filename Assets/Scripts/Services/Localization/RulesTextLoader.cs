using Common;
using TMPro;
using UnityEngine;
using Utils;

namespace Enums {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RulesTextLoader : MonoBehaviour, ISolitaireVariant
    {
        public void Configure(SolitaireKind kind)
        {
            GetComponent<TextMeshProUGUI>().text = CommonUtils.GetRules(kind);
        }
    }
}