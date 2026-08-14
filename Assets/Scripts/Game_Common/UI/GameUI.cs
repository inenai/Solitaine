using FreeCell;
using Klondike;
using Sawayama;
using TMPro;
using UnityEngine;
using Utils;

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
        [SerializeField] private GameObject _controlsBtn;
        [SerializeField] private GameObject _controlsScreen;

        private IGameController _controller;

        public void Init(IGameController controller)
        {
            ApplyTints();
#if UNITY_ANDROID && !UNITY_EDITOR
            _controlsBtn.SetActive(false);
#endif
            _controller = controller;
            _gameLabel.text = _solitaireKind.ToString();
            UpdateWins();
        }

        private void ApplyTints()
        {
            UIColorTinter[] tinters = GetComponentsInChildren<UIColorTinter>(true);
            foreach (UIColorTinter tinter in tinters)
            {
                tinter.SetColor(_solitaireKind);
            }
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
            int wins = CommonUtils.GetWinsFor(_solitaireKind);
            _winsTxt.text = $"Wins: {wins}";
        }

        public void OpenControlsScreen()
        {
            EventManager.OnMenuOpened?.Invoke();
            _controlsScreen.SetActive(true);
        }

        public void CloseControlsScreen()
        {
            _controlsScreen.SetActive(false);
            EventManager.OnMenuClosed?.Invoke();
        }
    }
}