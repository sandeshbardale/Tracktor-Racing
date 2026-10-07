using System.Collections.Generic;
using UnityEngine;
using BhootiyaRasta.Environment;
using BhootiyaRasta.Horror;
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.Core
{
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public string levelTitle;
        public string levelSubtitle;
        public float roadLength;
        public float roadCurvature;
        public int seed;
    }

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Configurations (60s Challenge)")]
        public List<LevelData> levels = new List<LevelData>();

        [Header("Scene References")]
        public ImportedRoadBuilder importedRoad;
        public ProceduralRoad roadGenerator;
        public RuralPropGenerator propGenerator;
        public RuralObstacleGenerator obstacleGenerator;
        public EnvironmentManager environmentManager;
        public GhostChaseManager ghostChaseManager;
        public TractorController tractor;

        public int CurrentLevelIndex { get; private set; } = 1;
        public int UnlockedLevel { get; private set; } = 1;

        private const string UNLOCKED_LEVEL_KEY = "BhootiyaRasta_UnlockedLevel";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            UnlockedLevel = PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY, 1);
            InitializeLevelData();
        }

        private void InitializeLevelData()
        {
            if (levels.Count > 0) return;

            // Strict 60-second driving lengths for 25-30 km/h (~7 m/s) agricultural tractor
            levels.Add(new LevelData
            {
                levelNumber = 1,
                levelTitle = "Level 1: The Farm Road",
                levelSubtitle = "Pehla Kadam - Shuruat",
                roadLength = 320f,
                roadCurvature = 10f,
                seed = 101
            });

            levels.Add(new LevelData
            {
                levelNumber = 2,
                levelTitle = "Level 2: The Fields",
                levelSubtitle = "Sunsaan Khet",
                roadLength = 360f,
                roadCurvature = 16f,
                seed = 202
            });

            levels.Add(new LevelData
            {
                levelNumber = 3,
                levelTitle = "Level 3: The Abandoned Farm",
                levelSubtitle = "Ujda Gaon",
                roadLength = 400f,
                roadCurvature = 22f,
                seed = 303
            });

            levels.Add(new LevelData
            {
                levelNumber = 4,
                levelTitle = "Level 4: The Forest Road",
                levelSubtitle = "Ghanghor Jungle",
                roadLength = 430f,
                roadCurvature = 28f,
                seed = 404
            });

            levels.Add(new LevelData
            {
                levelNumber = 5,
                levelTitle = "Level 5: Bhootiya Rasta",
                levelSubtitle = "Khooni Mod Aur Antim Safar",
                roadLength = 460f,
                roadCurvature = 35f,
                seed = 505
            });
        }

        public void LoadLevel(int levelNumber)
        {
            if (levels == null || levels.Count == 0)
            {
                InitializeLevelData();
            }

            CurrentLevelIndex = Mathf.Clamp(levelNumber, 1, levels.Count);
            LevelData data = levels[CurrentLevelIndex - 1];

            if (tractor == null) tractor = FindAnyObjectByType<TractorController>();
            if (importedRoad == null) importedRoad = FindAnyObjectByType<ImportedRoadBuilder>();
            if (obstacleGenerator == null) obstacleGenerator = FindAnyObjectByType<RuralObstacleGenerator>();
            if (propGenerator == null) propGenerator = FindAnyObjectByType<RuralPropGenerator>();
            if (environmentManager == null) environmentManager = FindAnyObjectByType<EnvironmentManager>();
            if (ghostChaseManager == null) ghostChaseManager = FindAnyObjectByType<GhostChaseManager>();

            // Generate Road
            if (importedRoad != null)
            {
                importedRoad.totalTrackLength = data.roadLength;
                importedRoad.curveAmplitude = data.roadCurvature;
                importedRoad.BuildImportedRoadEnvironment();

                // Spawn Obstacles & Destination Village Gate along road
                if (obstacleGenerator != null)
                {
                    obstacleGenerator.GenerateObstaclesForLevel(importedRoad.RoadPoints, CurrentLevelIndex);
                }

                // Reset Tractor position on imported road
                if (tractor != null && importedRoad.RoadPoints.Count > 1)
                {
                    Vector3 startPos = importedRoad.RoadPoints[0] + Vector3.up * 0.25f;
                    Vector3 nextPos = importedRoad.RoadPoints[1];
                    tractor.transform.position = startPos;
                    tractor.transform.rotation = Quaternion.LookRotation((nextPos - startPos).normalized);

                    var rb = tractor.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                }

                // Setup Destination Checkpoint
                if (CheckpointManager.Instance != null && importedRoad.RoadPoints.Count > 0)
                {
                    Vector3 start = importedRoad.RoadPoints[0];
                    Vector3 end = importedRoad.RoadPoints[importedRoad.RoadPoints.Count - 1];
                    CheckpointManager.Instance.SetupLevelDestination(start, end);
                }
            }

            // Apply Night Fog & Atmosphere
            if (environmentManager != null)
            {
                environmentManager.ApplyLevelAtmosphere(CurrentLevelIndex);
            }

            // Configure Ghost Chase Aggression
            if (ghostChaseManager != null)
            {
                ghostChaseManager.SetLevelAggression(CurrentLevelIndex);
            }
            else if (GhostChaseManager.Instance != null)
            {
                GhostChaseManager.Instance.SetLevelAggression(CurrentLevelIndex);
            }

            // Start 60s Challenge Countdown & Intro
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartLevelSequence();
            }
        }

        public void UnlockNextLevel()
        {
            if (CurrentLevelIndex >= UnlockedLevel && UnlockedLevel < 5)
            {
                UnlockedLevel = CurrentLevelIndex + 1;
                PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, UnlockedLevel);
                PlayerPrefs.Save();
            }
        }

        public LevelData GetCurrentLevelData()
        {
            if (CurrentLevelIndex >= 1 && CurrentLevelIndex <= levels.Count)
            {
                return levels[CurrentLevelIndex - 1];
            }
            return null;
        }
    }
}
