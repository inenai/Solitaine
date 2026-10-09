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
        [SerializeField] TextMeshProUGUI _thingToAnimate;
        [SerializeField] LoadGameConfirmPanel _confirmPanel;

        bool savedGameAvailable;
        System.Random rng;

        void Start()
        {
            rng = new System.Random();
            GetComponent<Button>().onClick.AddListener(GoToGame);
            UpdateWinShowcase(God.Database.GetTotalWins(_kind));
            if (God.Database.HasSavedGame(_kind))
            {
                savedGameAvailable = true;
            }
        }

        void Update()
        {
            if (savedGameAvailable)
            {
                _thingToAnimate.transform.localPosition = new Vector2(rng.Next(0, 100) / 30f, rng.Next(0, 100) / 50f);
            }
        }

        private void GoToGame()
        {
            if (savedGameAvailable)
            {
                PromptDecisionToLoadSavedGame();
                return;
            }
            GameNavigator.LoadGame(_kind);
        }

        private void UpdateWinShowcase(int wins)
        {
            _winsTxt.text = wins.ToString();
            _winsTxt.gameObject.SetActive(wins > 0);
            _winsIcon.color = CommonUtils.GetWinsColor(wins);
            _winsIcon.gameObject.SetActive(wins > 0);
        }

        private void PromptDecisionToLoadSavedGame()
        {
            _confirmPanel.OnDecisionMade += DecisionMade;
            _confirmPanel.gameObject.SetActive(true);
        }

        private void DecisionMade(bool loadGame)
        {
            _confirmPanel.OnDecisionMade -= DecisionMade;
            if (!loadGame) God.Database.TryDeleteSavedGame(_kind);
            GameNavigator.LoadGame(_kind);
        }

        void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
            _confirmPanel.OnDecisionMade -= DecisionMade;
        }
    }
}