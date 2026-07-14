using TMPro;
using UnityEngine;

namespace Common
{
    public abstract class GameUI : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI _winsTxt;
        [SerializeField] protected ParticleSystem _victoryParticles;
        [SerializeField] protected GameObject _settingsScreen;

        protected IGameController _controller;

        protected abstract void OnInit();

        public void Init(IGameController controller)
        {
            _controller = controller;
            OnInit();
        }

        public virtual void OnStartNewGame()
        {
            _victoryParticles.Stop();
        }

        public virtual void OnGameWon()
        {
            _victoryParticles.Play();
        }

        public void OpenSettings()
        {
            _settingsScreen.SetActive(true);
        }

        public void CloseSettings()
        {
            _settingsScreen.SetActive(false);
        }
    }
}