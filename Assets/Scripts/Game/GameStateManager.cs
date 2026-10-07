using System;
using UnityEngine;

namespace TractorRacing
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("State Settings")]
        [SerializeField]
        private GameState currentState = GameState.MainMenu;

        public GameState CurrentState => currentState;

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            Debug.Log($"[GameStateManager] State changed to: {newState}");
            OnStateChanged?.Invoke(currentState);
        }

        public bool IsPlaying() => currentState == GameState.Playing;
        public bool IsPaused() => currentState == GameState.Paused;
        public bool IsGameOver() => currentState == GameState.GameOver;
        public bool IsLevelComplete() => currentState == GameState.LevelCompleted || currentState == GameState.LevelComplete;
    }
}
