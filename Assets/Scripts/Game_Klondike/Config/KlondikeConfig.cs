using UnityEngine;

namespace Klondike
{
    [CreateAssetMenu(fileName = "KlondikeConfig", menuName = "Solitaine/Create Klondike config asset")]
    public class KlondikeConfig : ScriptableObject
    {
        [SerializeField] int _drawAmount = KlondikeSettings.DEFAULT_DRAW_AMOUNT;
        /// <summary>
        /// -1 for infinite
        /// </summary>
        [SerializeField] int _availableRestocks = KlondikeSettings.DEFAULT_RESTOCKS;
        [SerializeField] bool _foundationCardsFree = KlondikeSettings.DEFAULT_FOUNDATION_CARDS_FREE;
        [SerializeField] bool _autoMovesEnabled = KlondikeSettings.DEFAULT_AUTO_MOVES_ENABLED;

        public int DrawAmount => _drawAmount;
        public int AvailableRestocks => _availableRestocks;
        public bool FoundationCardsFree => _foundationCardsFree;
        public bool AutoMovesEnabled => _autoMovesEnabled;

        [ContextMenu("Apply default Klondike settings")]
        public void ApplyDefaultSettings()
        {
            KlondikeSettings.Reset(this);
        }
    }
}