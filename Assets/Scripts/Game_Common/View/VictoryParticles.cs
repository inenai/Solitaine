using UnityEngine;

namespace Common
{
    [RequireComponent (typeof(ParticleSystem))]
    public class VictoryParticles : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            EventManager.OnGameWon += OnGameWon;
            EventManager.OnGameStarted += OnGameStart;
        }

        private void OnGameWon()
        {
            _particleSystem.Play();
        }

        private void OnGameStart()
        {
            _particleSystem.Stop();
        }

        void OnDestroy()
        {
            EventManager.OnGameWon -= OnGameWon;
            EventManager.OnGameStarted -= OnGameStart;
        }

    }
}