using Klondike;
using Sawayama;
using TMPro;
using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(GameNavigator))]
    public class GameUI : MonoBehaviour, ISettingsManager
    {

        [SerializeField] private TextMeshProUGUI _gameLabel;
        [SerializeField] private SolitaireKind _solitaireKind;
        [SerializeField] private TextMeshProUGUI _winsTxt;
        [SerializeField] private ParticleSystem _victoryParticles;
        [SerializeField] private BaseSettingsScreen _settingsScreen;

        private GameNavigator _navi;
        private IGameController _controller;

        public void Init(IGameController controller)
        {
            _controller = controller;
            _navi = GetComponent<GameNavigator>();
            _gameLabel.text = _solitaireKind.ToString();
            UpdateWins();
        }

        public void OnStartNewGame()
        {
            _victoryParticles.Stop();
        }

        public void OnGameWon()
        {
            UpdateWins();
            _victoryParticles.Play();
        }

        public void OpenSettings()
        {
            _settingsScreen.gameObject.SetActive(true);
        }

        public void RestartGame()
        {
            _controller.RestartGame();
        }

        private void UpdateWins()
        {
            int wins = 0;
            switch (_solitaireKind)
            {
                case SolitaireKind.KLONDIKE:
                    wins = KlondikeSettings.WinCount;
                    break;
                case SolitaireKind.SAWAYAMA:
                    wins = SawayamaSettings.WinCount;
                    break;
            }
            _winsTxt.text = $"Wins: {wins}";
        }
    }
}