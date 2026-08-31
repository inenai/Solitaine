using UnityEngine;
namespace Common
{

    public class VariantConfigure : MonoBehaviour
    {
        [SerializeField] SolitaireKind _solitaireKind;
        void OnEnable()
        {
            ConfigureVariants();
        }

        private void ConfigureVariants()
        {
            ISolitaireVariant[] variants = GetComponents<ISolitaireVariant>();
            foreach(ISolitaireVariant v in variants)
            {
                v?.Configure(_solitaireKind);
            }
        }
    }
}