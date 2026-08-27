using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Common
{
    public class GameUI : MonoBehaviour, ISettingsManager
    {
        [SerializeField] private TextMeshProUGUI _gameLabel;
        [SerializeField] private SolitaireKind _solitaireKind;
        [SerializeField] private GameSettingsScreen _settingsScreen;
        [SerializeField] private GameObject _rulesCtrlsBtnsPanel;
        [SerializeField] private TextMeshProUGUI _controlsScreenTxt;
        [SerializeField] private Button _undoBtn;
        [SerializeField] private Button _redoBtn;
        [SerializeField] private GameObject _restartBtn;

        private GameController _controller;

        public void Init(GameController controller)
        {
            ConfigureVariants();
            _controller = controller;
            UpdateGameLabel();
#if UNITY_ANDROID
            _controlsScreenTxt.text = CommonUtils.GetTouchCtrlsText();
#else
            _controlsScreenTxt.text = CommonUtils.GetKeyboardMouseCtrlsDesc();
#endif
        }

        private void ConfigureVariants()
        {
            ISolitaireVariant[] variants = GetComponentsInChildren<ISolitaireVariant>(true);
            foreach (ISolitaireVariant variant in variants)
            {
                variant?.Configure(_solitaireKind);
            }
        }

        public void OnGameWon()
        {
            UpdateGameLabel();
            _restartBtn.SetActive(true);
        }

        public void OpenSettings()
        {
            _settingsScreen.gameObject.SetActive(true);
        }

        public void RestartGame()
        {
            _controller.RestartGame();
        }

        private void UpdateGameLabel()
        {
            int wins = CommonUtils.GetWinsFor(_solitaireKind);
            _gameLabel.text = _solitaireKind.ToString() + "\n" + $"Wins: {wins}";
        }

        public void Undo()
        {
            EventManager.OnUndo?.Invoke();
        }

        public void Redo()
        {
            EventManager.OnRedo?.Invoke();
        }

        public void UpdateUndoRedoButtons(bool enableUndoBtn, bool enableRedoBtn)
        {
            _undoBtn.interactable = enableUndoBtn;
            _redoBtn.interactable = enableRedoBtn;
        }
    }
}