using UnityEngine;

namespace TractorRacing
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        public bool isStartCheckpoint = false;
        public int checkpointIndex = 1;
        public bool isPassed = false;

        public Collider triggerCollider;
        private CheckpointManager checkpointManager;

        private void Awake()
        {
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            checkpointManager = GetComponentInParent<CheckpointManager>() ?? FindObjectOfType<CheckpointManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isPassed) return;

            // Check if player or any child wheel/collider entered
            if (other.CompareTag("Player") || other.transform.root.CompareTag("Player") || other.GetComponentInParent<TractorPhysics>() != null)
            {
                if (checkpointManager == null) checkpointManager = CheckpointManager.Instance ?? FindObjectOfType<CheckpointManager>();
                if (checkpointManager != null)
                {
                    isPassed = true;
                    checkpointManager.OnCheckpointPassed(this);
                    HighlightPassed();
                }
            }
        }

        public void HighlightPassed()
        {
            // Turn checkpoint posts green on pass
            var renderers = GetComponentsInChildren<Renderer>();
            Material greenMat = Resources.Load<Material>("Mat_Finish_Banner") ?? Resources.Load<Material>("Materials/Mat_Finish_Banner");
            foreach (var r in renderers)
            {
                r.material.color = new Color(0.1f, 0.9f, 0.2f);
                if (r.material.HasProperty("_EmissionColor"))
                {
                    r.material.EnableKeyword("_EMISSION");
                    r.material.SetColor("_EmissionColor", new Color(0.1f, 0.8f, 0.2f));
                }
            }
        }
    }
}
