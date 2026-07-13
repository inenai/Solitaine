using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Common
{
    [RequireComponent(typeof(Collider2D))]
    public class CardUIText : CardUI
    {

        [SerializeField] TextMeshPro[] _suitStr;
        [SerializeField] TextMeshPro[] _valueStr;
        [SerializeField] TextMeshPro[] _lightAlpha;

        public override void Refresh()
        {
            foreach (TextMeshPro txt in _suitStr)
            {
                txt.text = CardUtils.GetSuitStr(_card.Suit);
                txt.color = CardUtils.GetSuitColor(_card.Suit);
            }

            foreach (TextMeshPro txt in _valueStr)
            {
                txt.text = CardUtils.CardValue(_card);
            }

            foreach (TextMeshPro txt in _lightAlpha)
            {
                txt.color = txt.color.WithAlpha(0.5f);
            }

            base.Refresh();
        }
    }
}