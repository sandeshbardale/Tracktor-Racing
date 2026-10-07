using UnityEngine;

namespace TractorRacing
{
    public class TractorRacingCameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField]
        private Transform target;

        [SerializeField]
        private Vector3 followOffset = new Vector3(0f, 2.6f, -5.5f);

        [SerializeField]
        private float positionSmoothSpeed = 12f;

        [SerializeField]
        private float rotationSmoothSpeed = 10f;

        [SerializeField]
        private float cameraDistance = 5.5f;

        private TractorPhysics tractor;
        private bool isInitialized = false;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            FindTarget();
        }

        private void Start()
        {
            FindTarget();
            if (target != null)
            {
                SnapToTarget();
            }
        }

        private void FindTarget()
        {
            if (target != null) return;

            tractor = FindObjectOfType<TractorPhysics>();
            if (tractor != null)
            {
                target = tractor.transform;
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                return;
            }

            string[] candidateNames = new string[] { "Tractor_Racing", "IndianTractor_Bhootni" };
            foreach (var n in candidateNames)
            {
                var go = GameObject.Find(n);
                if (go != null)
                {
                    target = go.transform;
                    return;
                }
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Vector3 desiredPosition = target.position - target.forward * cameraDistance + Vector3.up * followOffset.y;
            transform.position = desiredPosition;
            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation((lookTarget - transform.position).normalized);
            isInitialized = true;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTarget();
                if (target == null) return;
            }

            if (!isInitialized)
            {
                SnapToTarget();
            }

            bool lookBehind = SafeInput.GetKey(KeyCode.V) || SafeInput.GetKey(KeyCode.B) || SafeInput.GetKey(KeyCode.C);

            Vector3 forwardDir = lookBehind ? -target.forward : target.forward;
            Vector3 desiredPosition = target.position - forwardDir * cameraDistance + Vector3.up * followOffset.y;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmoothSpeed);

            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            Vector3 dir = (lookTarget - transform.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmoothSpeed);
            }

            // Dynamic FOV based on speed
            if (cam != null && tractor != null)
            {
                float speedRatio = Mathf.Clamp01(tractor.CurrentSpeed / tractor.maxForwardSpeed);
                float targetFOV = Mathf.Lerp(60f, 72f, speedRatio);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null) SnapToTarget();
        }
    }
}

