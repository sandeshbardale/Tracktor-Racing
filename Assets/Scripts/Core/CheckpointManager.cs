using UnityEngine;
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.Core
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }

        [Header("Target & Goal")]
        public TractorController tractor;
        public Vector3 destinationPoint;
        public float totalDistance = 500f;
        public float completionRadius = 8f;

        private bool hasCompleted = false;

        public float DistanceRemaining { get; private set; }
        public float ProgressNormalized { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetupLevelDestination(Vector3 startPoint, Vector3 endPoint)
        {
            destinationPoint = endPoint;
            totalDistance = Vector3.Distance(startPoint, endPoint);
            hasCompleted = false;
        }

        private void Update()
        {
            if (tractor == null || hasCompleted) return;

            DistanceRemaining = Vector3.Distance(tractor.transform.position, destinationPoint);
            ProgressNormalized = Mathf.Clamp01(1f - (DistanceRemaining / totalDistance));

            if (DistanceRemaining <= completionRadius)
            {
                hasCompleted = true;
                GameManager.Instance?.OnLevelGoalReached();
            }
        }
    }
}
