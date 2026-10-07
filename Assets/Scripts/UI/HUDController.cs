using UnityEngine;
using UnityEngine.UI;
using TractorRacing;

namespace TractorRacing
{
    public class HUDController : MonoBehaviour
    {
        [Header("HUD References")]
        [SerializeField]
        private Text levelText;

        [SerializeField]
        private Text timerText;

        [SerializeField]
        private Text speedText;

        [SerializeField]
        private Text checkpointText;

        [SerializeField]
        private TractorPhysics tractor;

        private GameManager gameManager;
        private LevelManager levelManager;
        private CheckpointManager checkpointManager;
        private RaceTimer raceTimer;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            levelManager = FindObjectOfType<LevelManager>();
            checkpointManager = FindObjectOfType<CheckpointManager>();
            raceTimer = FindObjectOfType<RaceTimer>();

            if (tractor == null)
            {
                tractor = FindObjectOfType<TractorPhysics>();
            }

            if (levelText == null)
            {
                Transform t = transform.Find("LevelText");
                if (t != null) levelText = t.GetComponent<Text>();
            }

            if (timerText == null)
            {
                Transform t = transform.Find("TimerText");
                if (t != null) timerText = t.GetComponent<Text>();
            }

            if (speedText == null)
            {
                Transform t = transform.Find("SpeedText");
                if (t != null) speedText = t.GetComponent<Text>();
            }

            if (checkpointText == null)
            {
                Transform t = transform.Find("CheckpointText");
                if (t != null) checkpointText = t.GetComponent<Text>();
            }
        }

        private void Update()
        {
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();
            if (checkpointManager == null) checkpointManager = FindObjectOfType<CheckpointManager>();
            if (levelManager == null) levelManager = FindObjectOfType<LevelManager>();
            if (raceTimer == null) raceTimer = FindObjectOfType<RaceTimer>();

            if (levelText != null)
            {
                if (levelManager != null)
                {
                    levelText.text = "Level " + levelManager.CurrentLevelIndex;
                }
            }

            if (timerText != null && raceTimer != null)
            {
                timerText.text = "Time: " + raceTimer.GetTimeRemaining().ToString("F0") + "s";
            }

            if (speedText != null && tractor != null)
            {
                float speedKmh = tractor.CurrentSpeed * 3.6f;
                speedText.text = "Speed: " + speedKmh.ToString("F0") + " km/h";
            }

            if (checkpointText != null && checkpointManager != null)
            {
                int displayCp = Mathf.Min(checkpointManager.currentCheckpoint, checkpointManager.totalCheckpoints);
                checkpointText.text = $"Checkpoint: {displayCp}/{checkpointManager.totalCheckpoints}";
            }
        }
    }
}
