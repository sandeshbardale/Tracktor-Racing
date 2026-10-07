using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BhootiyaRasta.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class TractorController : MonoBehaviour
    {
        [Header("Engine & Drive Settings")]
        [SerializeField] private float acceleration = 25f;
        [SerializeField] private float maxForwardSpeed = 10f; // ~36 km/h realistic tractor top speed
        [SerializeField] private float maxReverseSpeed = 5f;
        [SerializeField] private float turnSpeed = 60f;
        [SerializeField] private float brakeForce = 22f;
        [SerializeField] private float dragCoefficient = 0.5f;

        [Header("Mud & Terrain")]
        [SerializeField] private float mudSlowdownMultiplier = 0.45f;
        private bool isInMud = false;

        [Header("Fuel System")]
        [SerializeField] private float maxFuel = 100f;
        [SerializeField] private float currentFuel = 100f;
        [SerializeField] private float fuelBurnRate = 0.5f;

        [Header("Lights")]
        [SerializeField] private Light[] headLights;
        [SerializeField] private Light[] tailLights;
        [SerializeField] private Color normalHeadlightColor = new Color(1f, 0.92f, 0.75f);
        [SerializeField] private Color ghostHeadlightColor = new Color(0.4f, 0.8f, 0.7f);
        private bool areHeadlightsOn = true;

        [Header("Visual References")]
        public Transform frontLeftWheel;
        public Transform frontRightWheel;
        public Transform rearLeftWheel;
        public Transform rearRightWheel;
        public Transform steeringWheel;
        public Transform tractorBody;
        public ParticleSystem mudSplatterParticles;
        public ParticleSystem exhaustSmoke;

        [Header("State")]
        private Rigidbody rb;
        private TractorAudio tractorAudio;
        private float throttleInput = 0f;
        private float steerInput = 0f;
        private float currentSteerAngle = 0f;
        private float frontWheelRollAngle = 0f;
        private float rearWheelRollAngle = 0f;
        private bool isBraking = false;
        private bool isEngineRunning = true;
        private bool isStalledByGhost = false;
        private Vector3 initialBodyLocalPos;

        public float CurrentSpeed => rb != null ? Vector3.Dot(rb.linearVelocity, transform.forward) : 0f;
        public float ThrottleInput => throttleInput;
        public float SteerInput => steerInput;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float CurrentFuel => currentFuel;
        public float MaxFuel => maxFuel;
        public bool IsEngineRunning => isEngineRunning;
        public bool IsInMud => isInMud;
        public bool AreHeadlightsOn => areHeadlightsOn;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            tractorAudio = GetComponent<TractorAudio>();
            rb.mass = 2200f; // Heavy Indian agricultural tractor
            rb.centerOfMass = new Vector3(0f, -0.4f, 0.1f); // Low center of gravity
            rb.linearDamping = dragCoefficient;
            rb.angularDamping = 3f;

            if (tractorBody != null)
            {
                initialBodyLocalPos = tractorBody.localPosition;
            }
        }

        private void Update()
        {
            ReadInput();
            HandleFuel();
            UpdateWheelVisuals();
            UpdateLights();
        }

        private void FixedUpdate()
        {
            if (!isEngineRunning) return;

            ApplyDrivePhysics();
        }

        private void ReadInput()
        {
            throttleInput = 0f;
            steerInput = 0f;
            isBraking = false;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) throttleInput += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) throttleInput -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) steerInput -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) steerInput += 1f;
            if (Input.GetKey(KeyCode.Space)) isBraking = true;

            if (Mathf.Abs(throttleInput) < 0.01f) throttleInput = Input.GetAxis("Vertical");
            if (Mathf.Abs(steerInput) < 0.01f) steerInput = Input.GetAxis("Horizontal");

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) throttleInput = Mathf.Max(throttleInput, 1f);
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) throttleInput = Mathf.Min(throttleInput, -1f);
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) steerInput = Mathf.Min(steerInput, -1f);
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) steerInput = Mathf.Max(steerInput, 1f);
                if (Keyboard.current.spaceKey.isPressed) isBraking = true;

                if (Keyboard.current.hKey.wasPressedThisFrame)
                {
                    tractorAudio?.PlayHorn();
                }

                if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.lKey.wasPressedThisFrame)
                {
                    ToggleHeadlights();
                }
            }
#endif

            if (Input.GetKeyDown(KeyCode.H))
            {
                tractorAudio?.PlayHorn();
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.L))
            {
                ToggleHeadlights();
            }
        }

        private void ApplyDrivePhysics()
        {
            float targetMaxSpeed = isInMud ? (maxForwardSpeed * mudSlowdownMultiplier) : maxForwardSpeed;
            float currentSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

            if (Mathf.Abs(throttleInput) > 0.05f)
            {
                float targetSpeed = throttleInput > 0 ? (throttleInput * targetMaxSpeed) : (throttleInput * maxReverseSpeed);
                float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
                rb.linearVelocity = transform.forward * newSpeed + new Vector3(0f, rb.linearVelocity.y, 0f);
            }
            else
            {
                float newSpeed = Mathf.MoveTowards(currentSpeed, 0f, (isBraking ? brakeForce * 2f : 10f) * Time.fixedDeltaTime);
                rb.linearVelocity = transform.forward * newSpeed + new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            if (isBraking)
            {
                float newSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * 2.5f * Time.fixedDeltaTime);
                rb.linearVelocity = transform.forward * newSpeed + new Vector3(0f, rb.linearVelocity.y, 0f);
            }

            // Steering
            if (Mathf.Abs(steerInput) > 0.05f)
            {
                float turnAngle = steerInput * turnSpeed * Time.fixedDeltaTime;
                if (currentSpeed < -0.5f) turnAngle = -turnAngle;
                Quaternion turnRotation = Quaternion.Euler(0f, turnAngle, 0f);
                rb.MoveRotation(rb.rotation * turnRotation);
            }
        }

        private void HandleFuel()
        {
            if (isEngineRunning && Mathf.Abs(throttleInput) > 0.1f)
            {
                currentFuel -= fuelBurnRate * Time.deltaTime;
                if (currentFuel <= 0f)
                {
                    currentFuel = 0f;
                    isEngineRunning = false;
                }
            }
        }

        private void AnimateChassisVibration()
        {
            if (tractorBody == null) return;

            if (isEngineRunning)
            {
                // Authentic single/two-cylinder tractor vibration
                float vibFreq = 30f + Mathf.Abs(CurrentSpeed) * 8f;
                float vibAmp = (0.008f + (isInMud ? 0.015f : 0f));
                float xOffset = Mathf.Sin(Time.time * vibFreq) * vibAmp * 0.6f;
                float yOffset = Mathf.Cos(Time.time * (vibFreq * 1.5f)) * vibAmp;
                tractorBody.localPosition = initialBodyLocalPos + new Vector3(xOffset, yOffset, 0f);
            }
            else
            {
                tractorBody.localPosition = Vector3.Lerp(tractorBody.localPosition, initialBodyLocalPos, Time.deltaTime * 5f);
            }
        }

        private void UpdateWheelVisuals()
        {
            float speed = CurrentSpeed;

            // Compute exact rotation increments based on real wheel radii
            float flRadius = 0.60f;
            float rlRadius = 0.74f;

            frontWheelRollAngle += (speed / flRadius) * Mathf.Rad2Deg * Time.deltaTime;
            rearWheelRollAngle += (speed / rlRadius) * Mathf.Rad2Deg * Time.deltaTime;

            // Keep angles within [-360, 360]
            if (frontWheelRollAngle > 3600f || frontWheelRollAngle < -3600f) frontWheelRollAngle %= 360f;
            if (rearWheelRollAngle > 3600f || rearWheelRollAngle < -3600f) rearWheelRollAngle %= 360f;

            // Front wheels steering turn angle with smooth interpolation
            float targetSteerAngle = steerInput * 30f;
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle, Time.deltaTime * 12f);

            // Apply rotation and yaw to front wheels only
            if (frontLeftWheel != null)
            {
                frontLeftWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(frontWheelRollAngle, 0f, 0f);
            }
            if (frontRightWheel != null)
            {
                frontRightWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f) * Quaternion.Euler(frontWheelRollAngle, 0f, 0f);
            }

            // Apply roll to rear wheels only
            if (rearLeftWheel != null)
            {
                rearLeftWheel.localRotation = Quaternion.Euler(rearWheelRollAngle, 0f, 0f);
            }
            if (rearRightWheel != null)
            {
                rearRightWheel.localRotation = Quaternion.Euler(rearWheelRollAngle, 0f, 0f);
            }

            // Mud particles
            if (mudSplatterParticles != null)
            {
                var emission = mudSplatterParticles.emission;
                emission.enabled = isInMud && Mathf.Abs(speed) > 1f;
            }
        }

        public void SetLights(Light[] head, Light[] tail)
        {
            headLights = head;
            tailLights = tail;
        }

        private void UpdateLights()
        {
            if (headLights != null)
            {
                foreach (var light in headLights)
                {
                    if (light != null) light.enabled = areHeadlightsOn && isEngineRunning;
                }
            }

            if (tailLights != null)
            {
                foreach (var light in tailLights)
                {
                    if (light != null)
                    {
                        light.enabled = areHeadlightsOn && isEngineRunning;
                        light.intensity = isBraking ? 2.5f : 0.8f;
                    }
                }
            }
        }

        public void ToggleHeadlights()
        {
            areHeadlightsOn = !areHeadlightsOn;
        }

        public void SetMudState(bool inMud)
        {
            isInMud = inMud;
        }

        /// <summary>
        /// Supernatural Scare: Headlights flicker wildly and turn an unholy greenish tint before returning.
        /// </summary>
        public void TriggerHeadlightFlicker(float duration = 3.5f)
        {
            StartCoroutine(FlickerRoutine(duration));
        }

        private IEnumerator FlickerRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bool state = Random.value > 0.45f;
                Color col = (Random.value > 0.6f) ? ghostHeadlightColor : normalHeadlightColor;

                if (headLights != null)
                {
                    foreach (var l in headLights)
                    {
                        if (l != null)
                        {
                            l.enabled = state;
                            l.color = col;
                            l.intensity = state ? Random.Range(1.5f, 4f) : 0f;
                        }
                    }
                }
                yield return new WaitForSeconds(Random.Range(0.04f, 0.14f));
            }

            // Restore normal headlights
            if (headLights != null)
            {
                foreach (var l in headLights)
                {
                    if (l != null)
                    {
                        l.enabled = areHeadlightsOn;
                        l.color = normalHeadlightColor;
                        l.intensity = 3.0f;
                    }
                }
            }
        }

        /// <summary>
        /// Supernatural Scare: Engine suddenly sputters, dies, and restarts after a terrifying silence.
        /// </summary>
        public void StallEngine(float duration = 4.0f)
        {
            if (!isStalledByGhost)
            {
                StartCoroutine(EngineStallRoutine(duration));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Check impact relative velocity
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 2.0f)
            {
                var cam = FindAnyObjectByType<CameraControl.CameraController>();
                if (impactForce > 6.0f)
                {
                    // Large collision: major speed loss, heavy shake
                    cam?.AddTrauma(0.85f);
                    rb.linearVelocity *= 0.35f;
                }
                else if (impactForce > 3.5f)
                {
                    // Medium collision
                    cam?.AddTrauma(0.55f);
                    rb.linearVelocity *= 0.6f;
                }
                else
                {
                    // Minor collision
                    cam?.AddTrauma(0.3f);
                    rb.linearVelocity *= 0.8f;
                }
            }
        }

        private IEnumerator EngineStallRoutine(float duration)
        {
            isStalledByGhost = true;
            isEngineRunning = false;
            tractorAudio?.PlaySputterSound();

            yield return new WaitForSeconds(duration);

            // Re-ignite engine
            isEngineRunning = true;
            isStalledByGhost = false;
        }
    }
}
