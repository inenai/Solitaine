using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Common
{
    [RequireComponent(typeof(Button))]
    public class MainMenuGameButton : MonoBehaviour
    {
        [SerializeField] SolitaireKind _kind;
        [SerializeField] Image _winsIcon;
        [SerializeField] TextMeshProUGUI _winsTxt;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(GoToGame);
            UpdateWinShowcase(God.Database.GetTotalWins(_kind));
        }

        private void GoToGame()
        {
            GameNavigator.LoadGame(_kind);
        }

        private void UpdateWinShowcase(int wins)
        {
            _winsTxt.text = wins.ToString();
            _winsTxt.gameObject.SetActive(wins > 0);
            _winsIcon.color = CommonUtils.GetWinsColor(wins);
            _winsIcon.gameObject.SetActive(wins > 0);
        }

        void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
}