using UnityEngine;

namespace Scorpion
{
    [CreateAssetMenu(fileName = "ScorpionConfig", menuName = "Solitaine/Create Scorpion config asset")]
    public class ScorpionConfig : ScriptableObject
    {
        [SerializeField] ScorpionVariant _variant = ScorpionSettings.DEFAULT_VARIANT;
        [SerializeField] int _suits = ScorpionSettings.DEFAULT_SUITS;

        public ScorpionVariant Variant => _variant;
        public int Suits => _suits;

        [ContextMenu("Apply default Scorpion settings")]
        public void ApplyDefaultSettings()
        {
            ScorpionSettings.Reset(this);
        }
    }
}