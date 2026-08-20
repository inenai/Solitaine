using UnityEngine;

namespace Common
{
    public abstract class GameSettingsScreen : MonoBehaviour
    {
        [SerializeField] public SolitaireKind Kind;
        public abstract void Save();
        void Awake()
        {
            ApplyTints();
        }

        private void ApplyTints()
        {
            UIColorTinter[] tinters = GetComponentsInChildren<UIColorTinter>(true);
            foreach (UIColorTinter tinter in tinters)
            {
                tinter.SetColor(Kind);
            }
        }

    }
}