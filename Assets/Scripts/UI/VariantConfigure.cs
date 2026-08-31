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
            foreach (ISolitaireVariant v in variants)
            {
                v?.Configure(_solitaireKind);
            }

            ISolitaireVariant[] variantsChildren = GetComponentsInChildren<ISolitaireVariant>(true);
            foreach (ISolitaireVariant v in variantsChildren)
            {
                v?.Configure(_solitaireKind);
            }
        }
    }
}