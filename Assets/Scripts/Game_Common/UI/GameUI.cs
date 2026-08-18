using System;
using FreeCell;
using Klondike;
using Sawayama;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Common
{
    [RequireComponent(typeof(GameNavigator))]
    public class GameUI : MonoBehaviour, ISettingsManager
    {

        [SerializeField] private TextMeshProUGUI _gameLabel;
        [SerializeField] private SolitaireKind _solitaireKind;
        [SerializeField] private TextMeshProUGUI _winsTxt;
        [SerializeField] private BaseSettingsScreen _settingsScreen;
        [SerializeField] private GameObject _controlsBtn;
        [SerializeField] private GameObject _controlsScreen;
        [SerializeField] private Button _undoBtn;
        [SerializeField] private Button _redoBtn;

        private GameController _controller;

        public void Init(GameController controller)
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

        public void OnGameWon()
        {
            UpdateWins();
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