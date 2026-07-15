using UnityEngine;

namespace Common
{
    public abstract class GameSettingsScreen : MonoBehaviour
    {
        [SerializeField] public SolitaireKind Kind;
        public abstract void Save();
        public abstract void OnEnabled();

    }
}