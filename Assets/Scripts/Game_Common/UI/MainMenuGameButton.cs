using TMPro;
using UnityEngine;
using Utils;

namespace Common
{
    public class MainMenuGameButton : MonoBehaviour
    {
        [SerializeField] SolitaireKind _kind;
        [SerializeField] GameObject _winIconGO;
        [SerializeField] TextMeshProUGUI _winsTxt;

        void Start()
        {
            UpdateWinShowcase(CommonUtils.GetWinsFor(_kind));
        }

        private void UpdateWinShowcase(int wins)
        {
            _winsTxt.text = wins.ToString();
            _winsTxt.gameObject.SetActive(wins > 0);
            _winIconGO.SetActive(wins > 0);
        }
    }
}