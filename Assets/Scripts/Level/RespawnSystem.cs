using UnityEngine;
using System.Collections;

namespace TractorRacing
{
    public class RespawnSystem : MonoBehaviour
    {
        [Header("Respawn Settings")]
        [SerializeField]
        private float resetDelay = 0.5f;

        [SerializeField]
        public TractorPhysics tractor;
        public CheckpointManager checkpointManager;
        public GameManager gameManager;

        private void Awake()
        {
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();
            if (checkpointManager == null) checkpointManager = FindObjectOfType<CheckpointManager>();
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        }

        private void Update()
        {
            if (SafeInput.GetKeyDown(KeyCode.R))
            {
                ResetTractor();
            }
        }

        public void ResetTractor()
        {
            if (tractor == null || checkpointManager == null) return;

            if (checkpointManager.checkpoints != null && checkpointManager.checkpoints.Length > 0)
            {
                int idx = Mathf.Clamp(checkpointManager.currentCheckpoint - 1, 0, checkpointManager.checkpoints.Length - 1);
                if (checkpointManager.checkpoints[idx] != null)
                {
                    tractor.ResetToCheckpoint(checkpointManager.checkpoints[idx]);
                }
            }
        }

        public void ResetTractorImmediate()
        {
            ResetTractor();
        }

        public void SetCheckpoint(int checkpointNumber)
        {
            if (checkpointManager != null)
            {
                checkpointManager.ActivateCheckpoint(checkpointNumber);
            }
        }
    }
}
