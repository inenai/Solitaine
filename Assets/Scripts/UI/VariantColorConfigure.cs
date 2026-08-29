using UnityEngine;
namespace Common
{

    public class VariantColorConfigure : MonoBehaviour
    {
        [SerializeField] SolitaireKind _solitaireKind;
        void Start()
        {
            ConfigureVariants();
        }

        private void ConfigureVariants()
        {
            ISolitaireVariant variant = GetComponent<ISolitaireVariant>();
            variant?.Configure(_solitaireKind);
        }
    }
}