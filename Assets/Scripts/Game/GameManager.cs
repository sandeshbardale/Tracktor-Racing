using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TractorRacing;

namespace TractorRacing
{
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        RaceStart,
        Playing,
        Paused,
        LevelCompleted,
        GameOver,
        LevelComplete
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("Race Settings")]
        [SerializeField]
        private float levelTimeLimit = 120f;

        private float timeRemaining;
        private bool isRaceStarted;
        private bool isPaused;

        [Header("Tractor Controller")]
        [SerializeField]
        private TractorPhysics tractor;

        [Header("Level Manager")]
        [SerializeField]
        private LevelManager levelManager;

        public float TimeRemaining { get { return timeRemaining; } }
        public float LevelTimeLimit { get { return levelTimeLimit; } }
        public bool IsRaceStarted { get { return isRaceStarted; } }

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
            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();

            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                CurrentState = GameState.MainMenu;
            }
            else if (SceneManager.GetActiveScene().name == "LevelSelect")
            {
                CurrentState = GameState.LevelSelect;
            }
            else
            {
                StartRaceSequence();
            }
        }

        public void StartRaceSequence()
        {
            Time.timeScale = 1f;
            timeRemaining = levelTimeLimit;
            isRaceStarted = false;
            CurrentState = GameState.RaceStart;

            StartCoroutine(RaceCountdownRoutine());
        }

        private IEnumerator RaceCountdownRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            AudioManager.Instance?.PlayCountdownBeep(false);
            yield return new WaitForSeconds(1f);
            AudioManager.Instance?.PlayCountdownBeep(false);
            yield return new WaitForSeconds(1f);
            AudioManager.Instance?.PlayCountdownBeep(false);
            yield return new WaitForSeconds(1f);
            AudioManager.Instance?.PlayCountdownBeep(true);
            yield return new WaitForSeconds(0.4f);

            isRaceStarted = true;
            CurrentState = GameState.Playing;
        }

        private void Update()
        {
            HandlePauseInput();

            if (CurrentState == GameState.Playing)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    TriggerGameOver();
                    return;
                }
            }

            if (isPaused && CurrentState == GameState.Paused)
            {
                Time.timeScale = 0f;
            }
        }

        private void HandlePauseInput()
        {
            if (SafeInput.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                CurrentState = GameState.Paused;
                Time.timeScale = 0f;
                isPaused = true;
                PauseMenu pm = FindObjectOfType<PauseMenu>(true);
                if (pm != null) pm.ShowPauseMenu(true);
            }
            else if (CurrentState == GameState.Paused)
            {
                CurrentState = GameState.Playing;
                Time.timeScale = 1f;
                isPaused = false;
                PauseMenu pm = FindObjectOfType<PauseMenu>(true);
                if (pm != null) pm.ShowPauseMenu(false);
            }
        }

        public void OnLevelGoalReached()
        {
            float finalTime = timeRemaining;

            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.UnlockNextLevel();
                levelManager.SaveBestTime(levelManager.CurrentLevelIndex, finalTime);
            }

            CurrentState = GameState.LevelCompleted;
            TriggerLevelComplete(finalTime);
        }

        public void TriggerLevelComplete(float finalTime)
        {
            Debug.Log("<color=#00FF00><b>[GameManager] Level Complete! Time remaining: " + finalTime.ToString("F1") + "s</b></color>");
            AudioManager.Instance?.PlayLevelComplete();

            if (levelManager != null)
            {
                levelManager.SaveBestTime(levelManager.CurrentLevelIndex, finalTime);
            }

            LevelCompleteUI lcUI = FindObjectOfType<LevelCompleteUI>(true);
            if (lcUI != null)
            {
                lcUI.gameObject.SetActive(true);
                lcUI.ShowLevelComplete(true);
            }

            CurrentState = GameState.LevelComplete;
        }

        public void TriggerGameOver()
        {
            CurrentState = GameState.GameOver;
            Time.timeScale = 0f;
        }

        public void NextLevel()
        {
            if (levelManager != null)
            {
                int next = levelManager.CurrentLevelIndex + 1;
                if (next <= 5)
                {
                    levelManager.LoadLevel(next);
                }
                else
                {
                    WinGame();
                }
            }
        }

        public void RestartLevel()
        {
            if (levelManager != null)
            {
                levelManager.LoadLevel(levelManager.CurrentLevelIndex);
            }
            StartRaceSequence();
        }

        public void SelectLevel(int level)
        {
            if (levelManager != null)
            {
                levelManager.LoadLevel(level);
            }
        }

        public void MainMenu()
        {
            SceneManager.LoadScene("MainMenu");
            Time.timeScale = 1f;
        }

        public void WinGame()
        {
            CurrentState = GameState.LevelComplete;
            Time.timeScale = 0f;
        }
    }
}
