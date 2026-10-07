using System.Collections.Generic;
using UnityEngine;

namespace TractorRacing
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Checkpoint References")]
        public TractorPhysics tractor;

        [Header("Checkpoint Configuration")]
        public Transform[] checkpoints;
        public int currentCheckpoint = 1;
        public int totalCheckpoints = 3;

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
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();
            if (checkpoints == null || checkpoints.Length == 0)
            {
                AutoFindCheckpoints();
            }
        }

        public void AutoFindCheckpoints()
        {
            Checkpoint[] found = FindObjectsOfType<Checkpoint>();
            if (found != null && found.Length > 0)
            {
                System.Array.Sort(found, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));
                List<Transform> list = new List<Transform>();
                for (int i = 0; i < found.Length; i++)
                {
                    found[i].checkpointIndex = i + 1;
                    list.Add(found[i].transform);
                }
                checkpoints = list.ToArray();
                totalCheckpoints = checkpoints.Length;
            }
        }

        public void InitializeCheckpoints(Transform[] checkpointTransforms)
        {
            checkpoints = checkpointTransforms;
            totalCheckpoints = checkpointTransforms != null ? checkpointTransforms.Length : 0;
            currentCheckpoint = 1;

            if (checkpoints != null)
            {
                for (int i = 0; i < checkpoints.Length; i++)
                {
                    if (checkpoints[i] != null)
                    {
                        Checkpoint cp = checkpoints[i].GetComponent<Checkpoint>();
                        if (cp != null) cp.checkpointIndex = i + 1;
                    }
                }
            }
        }

        public void OnCheckpointPassed(Checkpoint passedCheckpoint)
        {
            if (tractor == null) tractor = FindObjectOfType<TractorPhysics>();

            int cpNumber = passedCheckpoint.checkpointIndex;
            if (cpNumber >= currentCheckpoint)
            {
                currentCheckpoint = cpNumber + 1;
                AudioManager.Instance?.PlayCheckpointPassed();
                Debug.Log($"<color=#00FF88>[CheckpointManager] Checkpoint {cpNumber}/{totalCheckpoints} Cleared! Next is {currentCheckpoint}</color>");
            }
        }

        public void ActivateNextCheckpoint()
        {
            if (currentCheckpoint <= totalCheckpoints)
            {
                currentCheckpoint++;
            }
        }

        public Vector3 GetCurrentCheckpointPosition()
        {
            if (checkpoints == null || checkpoints.Length == 0) return Vector3.zero;

            int idx = Mathf.Clamp(currentCheckpoint - 2, 0, checkpoints.Length - 1);
            if (checkpoints[idx] != null)
            {
                return checkpoints[idx].position;
            }
            return Vector3.zero;
        }

        public void ActivateCheckpoint(int checkpointNumber)
        {
            if (checkpointNumber >= 1 && checkpointNumber <= totalCheckpoints + 1)
            {
                currentCheckpoint = checkpointNumber;
            }
        }
    }
}
