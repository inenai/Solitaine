using UnityEngine;

namespace Klondike
{
    [CreateAssetMenu(fileName = "KlondikeConfig", menuName = "Solitaine/Create Klondike config asset")]
    public class KlondikeConfig : ScriptableObject
    {
        [SerializeField] int _drawAmount = 3;
        /// <summary>
        /// -1 for infinite
        /// </summary>
        [SerializeField] int _availableRestocks = -1;
        [SerializeField] bool _foundationCardsFree = false;

        public int DrawAmount => _drawAmount;
        public int AvailableRestocks => _availableRestocks;
        public bool FoundationCardsFree => _foundationCardsFree;

        [ContextMenu("Apply default Klondike settings")]
        public void ApplyDefaultSettings()
        {
            KlondikeSettings.Reset(this);
        }
    }
}