using UnityEngine;

namespace TractorRacing
{
    public class TractorWheel : MonoBehaviour
    {
        [Header("Wheel Visuals")]
        [SerializeField]
        public Transform wheelMesh;

        [HideInInspector]
        public WheelCollider baseCollider;

        [Header("Physics Constants")]
        [SerializeField]
        public float tireFriction = 1.0f;

        [SerializeField]
        public float suspensionStiffness = 15f;

        private void Awake()
        {
            if (baseCollider == null)
            {
                baseCollider = GetComponent<WheelCollider>();
            }
        }

        public void UpdateVisuals()
        {
            if (baseCollider == null || wheelMesh == null) return;

            Vector3 wheelPos;
            Quaternion wheelRot;
            baseCollider.GetWorldPose(out wheelPos, out wheelRot);

            wheelMesh.position = wheelPos;
            wheelMesh.rotation = wheelRot;
        }
    }
}


