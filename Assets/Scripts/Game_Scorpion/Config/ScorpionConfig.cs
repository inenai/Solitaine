using UnityEngine;

namespace Scorpion
{
    [CreateAssetMenu(fileName = "ScorpionConfig", menuName = "Solitaine/Create Scorpion config asset")]
    public class ScorpionConfig : ScriptableObject
    {
        [SerializeField] ScorpionVariant _variant = ScorpionSettings.DEFAULT_VARIANT;

        public ScorpionVariant Variant => _variant;

        [ContextMenu("Apply default Klondike settings")]
        public void ApplyDefaultSettings()
        {
            ScorpionSettings.Reset(this);
        }
    }
}