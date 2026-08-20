using UnityEngine;
namespace Spider
{
    [CreateAssetMenu(fileName = "SpiderConfig", menuName = "Solitaine/Create Spider config asset")]
    public class SpiderConfig : ScriptableObject
    {
        [SerializeField] int _suits = SpiderSettings.DEFAULT_SUITS;

        public int Suits => _suits;

        [ContextMenu("Apply default Spider settings")]
        public void ApplyDefaultSettings()
        {
            SpiderSettings.Reset(this);
        }
    }
}