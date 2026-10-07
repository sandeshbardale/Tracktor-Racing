using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using BhootiyaRasta.UI;
using BhootiyaRasta.Vehicle;
using BhootiyaRasta.Horror;

namespace BhootiyaRasta.Core
{
    public enum GameState
    {
        MainMenu,
        IntroSequence,
        Playing,
        Paused,
        LevelCompleted,
        GameOver,
        EndingCutscene
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.IntroSequence;

        [Header("60-Second Challenge Timer")]
        [SerializeField] private float levelTimeLimit = 60.0f;
        private float timeRemaining = 60.0f;

        [Header("Intro Countdown")]
        private float introTimer = 4.5f;
        private string introCountdownText = "3";

        [Header("Tractor Check")]
        [SerializeField] private TractorController tractor;

        public float TimeRemaining => timeRemaining;
        public float LevelTimeLimit => levelTimeLimit;
        public string IntroCountdownText => introCountdownText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (tractor == null) tractor = FindAnyObjectByType<TractorController>();

            // Load Level 1 initially
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(1);
            }
            else
            {
                StartLevelSequence();
            }
        }

        public void StartLevelSequence()
        {
            Time.timeScale = 1f;
            timeRemaining = levelTimeLimit;
            introTimer = 4.2f;
            CurrentState = GameState.IntroSequence;
            introCountdownText = "3";

            if (tractor == null) tractor = FindAnyObjectByType<TractorController>();

            GhostChaseManager.Instance?.ResetChase();
            UIManager.Instance?.HideOverlays();

            StartCoroutine(IntroSequenceRoutine());
        }

        private IEnumerator IntroSequenceRoutine()
        {
            CurrentState = GameState.IntroSequence;

            // 3 Seconds countdown
            introCountdownText = "3";
            yield return new WaitForSeconds(1.0f);

            introCountdownText = "2";
            yield return new WaitForSeconds(1.0f);

            introCountdownText = "1";
            yield return new WaitForSeconds(1.0f);

            introCountdownText = "GO!";
            yield return new WaitForSeconds(0.6f);

            // Start gameplay, 60s timer, and ghost pursuit!
            CurrentState = GameState.Playing;
            GhostChaseManager.Instance?.StartChase();
        }

        private void Update()
        {
            HandlePauseInput();

            if (CurrentState == GameState.Playing)
            {
                // Strict 60-Second Real-Time Countdown
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    TriggerGameOver("TIME OUT — The road didn't let you escape. Darkness consumed you.");
                    return;
                }

                CheckFailConditions();
            }
        }

        private void HandlePauseInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
#endif
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                UIManager.Instance?.ShowPauseMenu(true);
            }
            else if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                UIManager.Instance?.ShowPauseMenu(false);
            }
        }

        private void CheckFailConditions()
        {
            if (CurrentState != GameState.Playing) return;

            // Fuel ran out in the midnight dark
            if (tractor != null && tractor.CurrentFuel <= 0.01f && !tractor.IsEngineRunning)
            {
                TriggerGameOver("The tractor ran out of fuel... You are stranded in the pitch dark alone with her.");
            }
        }

        public void OnLevelGoalReached()
        {
            if (CurrentState != GameState.Playing) return;

            int completedLvl = LevelManager.Instance != null ? LevelManager.Instance.CurrentLevelIndex : 1;
            float finalTimeRemaining = timeRemaining;

            GhostChaseManager.Instance?.ResetChase();

            if (completedLvl >= 5)
            {
                // Final Ending!
                CurrentState = GameState.EndingCutscene;
                UIManager.Instance?.ShowFinalVictoryScreen(finalTimeRemaining);
            }
            else
            {
                CurrentState = GameState.LevelCompleted;
                LevelManager.Instance?.UnlockNextLevel();
                UIManager.Instance?.ShowLevelCompleteScreen(completedLvl, finalTimeRemaining);
            }
        }

        public void TriggerGameOver(string reason)
        {
            CurrentState = GameState.GameOver;
            GhostChaseManager.Instance?.ResetChase();
            UIManager.Instance?.ShowGameOverScreen(reason);
        }

        public void NextLevel()
        {
            if (LevelManager.Instance != null)
            {
                int next = LevelManager.Instance.CurrentLevelIndex + 1;
                LevelManager.Instance.LoadLevel(next);
            }
            StartLevelSequence();
        }

        public void RestartCurrentLevel()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(LevelManager.Instance.CurrentLevelIndex);
            }
            StartLevelSequence();
        }

        public void SelectLevel(int level)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(level);
            }
            StartLevelSequence();
        }
    }
}
