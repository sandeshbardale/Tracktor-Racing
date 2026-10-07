using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TractorRacing
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Configurations")]
        public List<LevelData> levels = new List<LevelData>();

        public int CurrentLevelIndex { get; private set; } = 1;
        public int UnlockedLevel { get; private set; } = 1;

        private const string UNLOCKED_LEVEL_KEY = "BhootiyaRacing_UnlockedLevel";
        private const string BEST_TIME_PREFIX = "BhootiyaRacing_BestTime_";

        private SaveManager saveManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            UnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
            saveManager = FindObjectOfType<SaveManager>();
            InitializeLevelData();
        }

        private void InitializeLevelData()
        {
            if (levels.Count > 0) return;

            levels.Add(new LevelData
            {
                levelNumber = 1,
                levelName = "Village Road",
                sceneName = "Level1",
                timeLimit = 120f,
                difficulty = "Easy",
                checkpointCount = 3,
                bestTime = GetBestTime(1),
                startPosition = new Vector3(0f, 0.5f, 0f),
                finishPosition = new Vector3(0f, 0.5f, 300f)
            });

            levels.Add(new LevelData
            {
                levelNumber = 2,
                levelName = "Mud Track",
                sceneName = "Level2",
                timeLimit = 120f,
                difficulty = "Medium",
                checkpointCount = 3,
                bestTime = GetBestTime(2),
                startPosition = new Vector3(0f, 0.5f, 0f),
                finishPosition = new Vector3(0f, 0.5f, 350f)
            });

            levels.Add(new LevelData
            {
                levelNumber = 3,
                levelName = "Mountain Road",
                sceneName = "Level3",
                timeLimit = 110f,
                difficulty = "Hard",
                checkpointCount = 4,
                bestTime = GetBestTime(3),
                startPosition = new Vector3(0f, 0.5f, 0f),
                finishPosition = new Vector3(0f, 0.5f, 400f)
            });

            levels.Add(new LevelData
            {
                levelNumber = 4,
                levelName = "Obstacle Track",
                sceneName = "Level4",
                timeLimit = 100f,
                difficulty = "Very Hard",
                checkpointCount = 4,
                bestTime = GetBestTime(4),
                startPosition = new Vector3(0f, 0.5f, 0f),
                finishPosition = new Vector3(0f, 0.5f, 430f)
            });

            levels.Add(new LevelData
            {
                levelNumber = 5,
                levelName = "Tractor Challenge",
                sceneName = "Level5",
                timeLimit = 120f,
                difficulty = "Extreme",
                checkpointCount = 5,
                bestTime = GetBestTime(5),
                startPosition = new Vector3(0f, 0.5f, 0f),
                finishPosition = new Vector3(0f, 0.5f, 460f)
            });
        }

        public LevelData GetCurrentLevelData()
        {
            if (CurrentLevelIndex >= 1 && CurrentLevelIndex <= levels.Count)
            {
                return levels[CurrentLevelIndex - 1];
            }
            return null;
        }

        public void LoadLevel(int levelNumber)
        {
            if (levels == null || levels.Count == 0)
            {
                InitializeLevelData();
            }

            CurrentLevelIndex = Mathf.Clamp(levelNumber, 1, levels.Count);
            LevelData data = levels[CurrentLevelIndex - 1];

            if (IsLevelUnlocked(levelNumber))
            {
                SceneManager.LoadScene(data.sceneName);
            }
            else
            {
                Debug.LogWarning($"Level {levelNumber} is locked. Current unlocked level: {UnlockedLevel}");
            }
        }

        public bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= UnlockedLevel;
        }

        public void UnlockNextLevel()
        {
            if (CurrentLevelIndex >= levels.Count) return;

            int nextLevel = CurrentLevelIndex + 1;
            if (nextLevel > UnlockedLevel && nextLevel <= 5)
            {
                UnlockedLevel = nextLevel;
                PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, UnlockedLevel);
                PlayerPrefs.Save();
            }
        }

        public float GetBestTime(int levelNumber)
        {
            return PlayerPrefs.GetFloat(BEST_TIME_PREFIX + levelNumber, 9999f);
        }

        public void SaveBestTime(int levelNumber, float time)
        {
            if (time < GetBestTime(levelNumber))
            {
                PlayerPrefs.SetFloat(BEST_TIME_PREFIX + levelNumber, time);
                PlayerPrefs.Save();
            }
        }
    }
}
