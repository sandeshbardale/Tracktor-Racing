using UnityEngine;

namespace TractorRacing
{
    public class FinishLine : MonoBehaviour
    {
        [Header("Finish Line Settings")]
        public bool hasTriggered = false;

        private GameManager gameManager;
        private CheckpointManager checkpointManager;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            checkpointManager = FindObjectOfType<CheckpointManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            // Check if player or child collider entered finish line
            if (other.CompareTag("Player") || other.transform.root.CompareTag("Player") || other.GetComponentInParent<TractorPhysics>() != null)
            {
                hasTriggered = true;
                Debug.Log("<color=#00FF00><b>[FinishLine] VICTORY! Finish Line Crossed!</b></color>");

                if (checkpointManager == null) checkpointManager = CheckpointManager.Instance ?? FindObjectOfType<CheckpointManager>();
                if (checkpointManager != null)
                {
                    checkpointManager.currentCheckpoint = checkpointManager.totalCheckpoints + 1;
                }

                if (gameManager == null) gameManager = GameManager.Instance ?? FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    gameManager.OnLevelGoalReached();
                }
                else
                {
                    // Direct fallback if GameManager instance is missing
                    LevelCompleteUI winUI = FindObjectOfType<LevelCompleteUI>(true);
                    if (winUI != null)
                    {
                        winUI.gameObject.SetActive(true);
                        winUI.ShowLevelComplete(true);
                    }
                    AudioManager.Instance?.PlayLevelComplete();
                }
            }
        }
    }
}
