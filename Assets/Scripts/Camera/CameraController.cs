using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using BhootiyaRasta.Vehicle;

namespace BhootiyaRasta.CameraControl
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Offsets")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 2.7f, -5.8f);
        [SerializeField] private Vector3 lookBackOffset = new Vector3(0f, 2.5f, 5.2f);
        [SerializeField] private float positionSmoothSpeed = 10f;
        [SerializeField] private float rotationSmoothSpeed = 8f;

        [Header("Camera Shake")]
        [SerializeField] private float roadShakeMultiplier = 0.04f;
        private float trauma = 0f; // 0 to 1 trauma for horror shakes
        private float traumaDecay = 1.2f;

        private TractorController tractorController;
        private bool isLookingBack = false;
        public bool IsLookingBack => isLookingBack;

        private bool isInitialized = false;

        private void Awake()
        {
            FindTargetIfNull();
        }

        private void Start()
        {
            FindTargetIfNull();
            if (target != null)
            {
                SnapToTarget();
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                tractorController = target.GetComponent<TractorController>();
                SnapToTarget();
            }
        }

        public void FindTargetIfNull()
        {
            if (target != null) return;

            // 1. Check tag "Player"
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                tractorController = player.GetComponent<TractorController>();
                return;
            }

            // 2. Check TractorController
            var tc = FindObjectOfType<TractorController>();
            if (tc != null)
            {
                target = tc.transform;
                tractorController = tc;
                return;
            }

            // 3. Check by name
            string[] candidateNames = new string[] { "IndianTractor_Bhootni", "Tractor_Racing", "IndianTractor_Mahindra" };
            foreach (var n in candidateNames)
            {
                var go = GameObject.Find(n);
                if (go != null)
                {
                    target = go.transform;
                    tractorController = go.GetComponent<TractorController>();
                    return;
                }
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            Vector3 chosenOffset = isLookingBack ? lookBackOffset : followOffset;
            transform.position = target.position + target.TransformDirection(chosenOffset);
            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            transform.rotation = Quaternion.LookRotation((lookTarget - transform.position).normalized, Vector3.up);
            isInitialized = true;
        }

        private void Update()
        {
            HandleLookBackInput();

            // Decay trauma over time
            if (trauma > 0f)
            {
                trauma = Mathf.Clamp01(trauma - traumaDecay * Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTargetIfNull();
                if (target == null) return;
            }

            if (!isInitialized)
            {
                SnapToTarget();
            }

            Vector3 chosenOffset = isLookingBack ? lookBackOffset : followOffset;

            // Target position based on tractor rotation
            Vector3 desiredPosition = target.position + target.TransformDirection(chosenOffset);
            
            // Add subtle terrain bump shake based on tractor speed
            float speed = (tractorController != null) ? Mathf.Abs(tractorController.CurrentSpeed) : 0f;
            float bumpShake = (speed * roadShakeMultiplier);
            Vector3 terrainJitter = new Vector3(
                Mathf.PerlinNoise(Time.time * 15f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, Time.time * 18f) - 0.5f,
                0f
            ) * bumpShake;

            // Add trauma shake (horror jumpscares / obstacles)
            float shakePower = trauma * trauma;
            Vector3 traumaJitter = new Vector3(
                (Mathf.PerlinNoise(Time.time * 25f, 10f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(Time.time * 25f, 20f) - 0.5f) * 2f,
                (Mathf.PerlinNoise(Time.time * 25f, 30f) - 0.5f) * 2f
            ) * (shakePower * 0.7f);

            desiredPosition += terrainJitter + traumaJitter;

            // Smooth position interpolation
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionSmoothSpeed);

            // Look rotation
            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            if (isLookingBack)
            {
                // Look behind the tractor
                lookTarget = target.position - target.forward * 10f + Vector3.up * 1.2f;
            }

            Vector3 direction = (lookTarget - transform.position).normalized;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

                // Add slight trauma roll/pitch
                float traumaRot = (Mathf.PerlinNoise(Time.time * 20f, 40f) - 0.5f) * 12f * shakePower;
                desiredRotation *= Quaternion.Euler(0f, 0f, traumaRot);

                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSmoothSpeed);
            }
        }

        private void HandleLookBackInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                isLookingBack = Keyboard.current.vKey.isPressed ||
                                Keyboard.current.bKey.isPressed ||
                                Keyboard.current.cKey.isPressed;
            }
            if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            {
                isLookingBack = true;
            }
#else
            isLookingBack = Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.B) || Input.GetKey(KeyCode.C) || Input.GetMouseButton(1);
#endif
        }

        /// <summary>
        /// Adds camera trauma (0 to 1) for horror shocks, collisions, or sudden ghost appearances.
        /// </summary>
        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
        }
    }
}
