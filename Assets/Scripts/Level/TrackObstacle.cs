using UnityEngine;

namespace TractorRacing
{
    public enum ObstacleType
    {
        Roadblock,
        MudPatch,
        HayBale,
        FallenLog
    }

    public class TrackObstacle : MonoBehaviour
    {
        [Header("Obstacle Settings")]
        public ObstacleType obstacleType = ObstacleType.Roadblock;
        public float mudSlowdownFactor = 0.55f;

        private void OnTriggerEnter(Collider other)
        {
            if (obstacleType == ObstacleType.MudPatch)
            {
                TractorPhysics tp = other.GetComponentInParent<TractorPhysics>();
                if (tp != null)
                {
                    tp.linearDamping = 1.8f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (obstacleType == ObstacleType.MudPatch)
            {
                TractorPhysics tp = other.GetComponentInParent<TractorPhysics>();
                if (tp != null)
                {
                    tp.linearDamping = 0.25f;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            TractorPhysics tp = collision.collider.GetComponentInParent<TractorPhysics>();
            if (tp != null)
            {
                // Play impact feedback sound
                AudioManager.Instance?.PlayCheckpointPassed();
            }
        }
    }
}
