using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TractorRacing
{
    [RequireComponent(typeof(Rigidbody))]
    public class TractorPhysics : MonoBehaviour
    {
        [Header("Engine & Drive Settings")]
        [SerializeField] public float acceleration = 35f;
        [SerializeField] public float maxForwardSpeed = 18f; // ~65 km/h
        [SerializeField] public float maxReverseSpeed = 10f; // ~36 km/h
        [SerializeField] public float brakeForce = 45f;
        [SerializeField] public float turnSpeed = 65f;

        [Header("Physics & Stability")]
        [SerializeField] public float mass = 2200f;
        [SerializeField] public float centerOfMassY = -0.35f;
        [SerializeField] public float angularDamping = 3.5f;
        [SerializeField] public float linearDamping = 0.25f;

        [Header("Wheel Transforms")]
        public Transform frontLeftWheel;
        public Transform frontRightWheel;
        public Transform rearLeftWheel;
        public Transform rearRightWheel;
        public Transform steeringWheel;

        private Rigidbody rb;
        private bool isEngineRunning = true;
        private float currentSpeed;
        private float throttleInput;
        private float steerInput;
        private bool isBraking;
        private float frontWheelRollAngle = 0f;
        private float rearWheelRollAngle = 0f;
        private float currentSteerAngle = 0f;

        // On-screen UI fallback driving inputs
        private float guiThrottle = 0f;
        private float guiSteer = 0f;
        private bool guiBrake = false;

        public float CurrentSpeed => currentSpeed;
        public float SpeedKmh => currentSpeed * 3.6f;
        public float ThrottleInput => throttleInput;
        public float SteerInput => steerInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = mass;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearDamping = linearDamping;
            rb.angularDamping = angularDamping;
            rb.centerOfMass = new Vector3(0f, centerOfMassY, 0.05f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            AutoFindWheelReferences();
        }

        private void Start()
        {
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.WakeUp();
            }
            isEngineRunning = true;
        }

        private void AutoFindWheelReferences()
        {
            if (frontLeftWheel == null)
            {
                Transform t = transform.Find("Wheel_FrontLeft") ?? transform.Find("VisualModel/Wheel_FrontLeft") ?? transform.Find("IndianTractor_Wheel_FL");
                if (t != null) frontLeftWheel = t;
            }
            if (frontRightWheel == null)
            {
                Transform t = transform.Find("Wheel_FrontRight") ?? transform.Find("VisualModel/Wheel_FrontRight") ?? transform.Find("IndianTractor_Wheel_FR");
                if (t != null) frontRightWheel = t;
            }
            if (rearLeftWheel == null)
            {
                Transform t = transform.Find("Wheel_RearLeft") ?? transform.Find("VisualModel/Wheel_RearLeft") ?? transform.Find("IndianTractor_Wheel_RL");
                if (t != null) rearLeftWheel = t;
            }
            if (rearRightWheel == null)
            {
                Transform t = transform.Find("Wheel_RearRight") ?? transform.Find("VisualModel/Wheel_RearRight") ?? transform.Find("IndianTractor_Wheel_RR");
                if (t != null) rearRightWheel = t;
            }
            if (steeringWheel == null)
            {
                Transform t = transform.Find("VisualModel/SteeringWheel_Mesh") ?? transform.Find("SteeringWheel_Mesh") ?? transform.Find("IndianTractor_SteeringWheel");
                if (t != null) steeringWheel = t;
            }
        }

        private void Update()
        {
            ReadPlayerInput();

            if (isEngineRunning)
            {
                float speedRatio = (maxForwardSpeed > 0f) ? Mathf.Clamp01(currentSpeed / maxForwardSpeed) : 0f;
                AudioManager.Instance?.SetEnginePitch(speedRatio);

                if (SafeInput.GetKeyDown(KeyCode.H))
                {
                    AudioManager.Instance?.PlayHorn();
                }

                if (SafeInput.GetKeyDown(KeyCode.R))
                {
                    QuickRespawnOnTrack();
                }
            }

            UpdateVisualWheelRotations();
        }

        private void ReadPlayerInput()
        {
            throttleInput = 0f;
            steerInput = 0f;
            isBraking = false;

            // 1. Safe Keyboard W/S/A/D & Arrows via SafeInput
            if (SafeInput.GetKey(KeyCode.W) || SafeInput.GetKey(KeyCode.UpArrow)) throttleInput += 1f;
            if (SafeInput.GetKey(KeyCode.S) || SafeInput.GetKey(KeyCode.DownArrow)) throttleInput -= 1f;
            if (SafeInput.GetKey(KeyCode.A) || SafeInput.GetKey(KeyCode.LeftArrow)) steerInput -= 1f;
            if (SafeInput.GetKey(KeyCode.D) || SafeInput.GetKey(KeyCode.RightArrow)) steerInput += 1f;
            if (SafeInput.GetKey(KeyCode.Space)) isBraking = true;

            // 2. Safe Axis Fallback
            if (Mathf.Abs(throttleInput) < 0.01f)
            {
                throttleInput = SafeInput.GetVerticalAxis();
            }
            if (Mathf.Abs(steerInput) < 0.01f)
            {
                steerInput = SafeInput.GetHorizontalAxis();
            }

            // 3. Incorporate On-Screen GUI inputs
            if (Mathf.Abs(guiThrottle) > 0.01f) throttleInput = guiThrottle;
            if (Mathf.Abs(guiSteer) > 0.01f) steerInput = guiSteer;
            if (guiBrake) isBraking = true;
        }

        private void FixedUpdate()
        {
            if (rb == null) return;
            if (rb.isKinematic) rb.isKinematic = false;
            if (!isEngineRunning) return;

            float currentFwdSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            currentSpeed = rb.linearVelocity.magnitude;

            // 1. Throttle / Acceleration / Reverse / Brake
            if (isBraking)
            {
                float newFwd = Mathf.MoveTowards(currentFwdSpeed, 0f, brakeForce * 3f * Time.fixedDeltaTime);
                rb.linearVelocity = transform.forward * newFwd + new Vector3(0f, rb.linearVelocity.y, 0f);
            }
            else if (Mathf.Abs(throttleInput) > 0.05f)
            {
                float targetSpeed = (throttleInput > 0f) ? (throttleInput * maxForwardSpeed) : (throttleInput * maxReverseSpeed);
                float newFwd = Mathf.MoveTowards(currentFwdSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
                
                // Directly set the horizontal velocity vector along tractor facing direction
                Vector3 horizontalVel = transform.forward * newFwd;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);

                // Add acceleration force
                rb.AddForce(transform.forward * throttleInput * acceleration * 1200f, ForceMode.Force);
            }
            else
            {
                // Engine rolling drag deceleration
                float newFwd = Mathf.MoveTowards(currentFwdSpeed, 0f, 6.0f * Time.fixedDeltaTime);
                Vector3 horizontalVel = transform.forward * newFwd;
                rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
            }

            // 2. Steering & Turning
            if (Mathf.Abs(steerInput) > 0.05f)
            {
                // Turn naturally in forward & reverse
                float dirFactor = (throttleInput < -0.1f || currentFwdSpeed < -0.3f) ? -1f : 1f;
                float turnAmount = steerInput * turnSpeed * dirFactor * Time.fixedDeltaTime;

                Quaternion turnRot = Quaternion.Euler(0f, turnAmount, 0f);
                rb.MoveRotation(rb.rotation * turnRot);
            }

            // 3. Lateral Grip (Dampen sideways drifting)
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            localVel.x *= 0.82f;
            rb.linearVelocity = transform.TransformDirection(localVel);
        }

        private void UpdateVisualWheelRotations()
        {
            float fwdSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
            float dt = Time.deltaTime;

            // Spin wheels based on travel speed
            frontWheelRollAngle += (fwdSpeed / 0.595f) * Mathf.Rad2Deg * dt;
            rearWheelRollAngle  += (fwdSpeed / 0.740f) * Mathf.Rad2Deg * dt;

            // Steer front wheels
            currentSteerAngle = Mathf.Lerp(currentSteerAngle, steerInput * 30f, dt * 12f);

            // Front Left Wheel
            if (frontLeftWheel != null)
            {
                frontLeftWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                Transform mesh = frontLeftWheel.Find("Wheel_FrontLeft_Mesh") ?? (frontLeftWheel.childCount > 0 ? frontLeftWheel.GetChild(0) : null);
                if (mesh != null) mesh.localRotation = Quaternion.Euler(frontWheelRollAngle, 0f, 0f);
            }

            // Front Right Wheel
            if (frontRightWheel != null)
            {
                frontRightWheel.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
                Transform mesh = frontRightWheel.Find("Wheel_FrontRight_Mesh") ?? (frontRightWheel.childCount > 0 ? frontRightWheel.GetChild(0) : null);
                if (mesh != null) mesh.localRotation = Quaternion.Euler(frontWheelRollAngle, 0f, 0f);
            }

            // Rear Left Wheel
            if (rearLeftWheel != null)
            {
                Transform mesh = rearLeftWheel.Find("Wheel_RearLeft_Mesh") ?? (rearLeftWheel.childCount > 0 ? rearLeftWheel.GetChild(0) : null);
                if (mesh != null) mesh.localRotation = Quaternion.Euler(rearWheelRollAngle, 0f, 0f);
            }

            // Rear Right Wheel
            if (rearRightWheel != null)
            {
                Transform mesh = rearRightWheel.Find("Wheel_RearRight_Mesh") ?? (rearRightWheel.childCount > 0 ? rearRightWheel.GetChild(0) : null);
                if (mesh != null) mesh.localRotation = Quaternion.Euler(rearWheelRollAngle, 0f, 0f);
            }

            // Steering Wheel
            if (steeringWheel != null)
            {
                steeringWheel.localRotation = Quaternion.Euler(20f, 0f, -currentSteerAngle * 3.5f);
            }
        }

        private void OnGUI()
        {
            // On-screen Touch / Click Controls in bottom corners
            GUIStyle ctrlStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            ctrlStyle.normal.textColor = Color.yellow;

            float btnSize = 65f;
            float margin = 20f;
            float bottomY = Screen.height - btnSize - margin;

            guiThrottle = 0f;
            guiSteer = 0f;
            guiBrake = false;

            // Left side: Steer Left / Right
            if (GUI.RepeatButton(new Rect(margin, bottomY, btnSize, btnSize), "◀ A", ctrlStyle)) guiSteer -= 1f;
            if (GUI.RepeatButton(new Rect(margin + btnSize + 10f, bottomY, btnSize, btnSize), "D ▶", ctrlStyle)) guiSteer += 1f;

            // Right side: Forward (W) / Reverse (S) / Brake
            float rightX = Screen.width - margin - btnSize;
            if (GUI.RepeatButton(new Rect(rightX, bottomY - btnSize - 10f, btnSize, btnSize), "▲ W", ctrlStyle)) guiThrottle += 1f;
            if (GUI.RepeatButton(new Rect(rightX, bottomY, btnSize, btnSize), "▼ S", ctrlStyle)) guiThrottle -= 1f;
            if (GUI.RepeatButton(new Rect(rightX - btnSize - 10f, bottomY, btnSize, btnSize), "BRAKE", ctrlStyle)) guiBrake = true;

            // Quick Respawn Button [R] & Horn [H]
            if (GUI.Button(new Rect(Screen.width * 0.5f - 110f, Screen.height - 45f, 100f, 35f), "📢 Horn [H]", ctrlStyle))
            {
                AudioManager.Instance?.PlayHorn();
            }
            if (GUI.Button(new Rect(Screen.width * 0.5f + 10f, Screen.height - 45f, 100f, 35f), "🔄 Reset [R]", ctrlStyle))
            {
                QuickRespawnOnTrack();
            }
        }

        public void QuickRespawnOnTrack()
        {
            Vector3 currentPos = transform.position;
            transform.position = new Vector3(0f, currentPos.y + 0.6f, currentPos.z);
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }
        }

        public void ResetToCheckpoint(Transform checkpointTransform)
        {
            if (checkpointTransform == null) return;

            transform.position = checkpointTransform.position + Vector3.up * 0.45f;
            transform.rotation = checkpointTransform.rotation;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }
            isEngineRunning = true;
        }

        public void SetEngineRunning(bool running)
        {
            isEngineRunning = running;
            if (!running && rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }

        public bool IsEngineRunning() => isEngineRunning;
    }
}
