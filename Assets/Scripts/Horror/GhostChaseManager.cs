using System.Collections;
using UnityEngine;
using BhootiyaRasta.Core;
using BhootiyaRasta.Vehicle;
using BhootiyaRasta.Environment;
using BhootiyaRasta.CameraControl;
using BhootiyaRasta.Audio;
using BhootiyaRasta.UI;

namespace BhootiyaRasta.Horror
{
    public class GhostChaseManager : MonoBehaviour
    {
        public static GhostChaseManager Instance { get; private set; }

        [Header("Target References")]
        public TractorController tractor;
        public ImportedRoadBuilder road;
        public GhostController ghost;

        [Header("Chase Dynamics")]
        [SerializeField] private float minChaseDistance = 3.0f;
        [SerializeField] private float maxChaseDistance = 45.0f;
        [SerializeField] private float catchDistance = 2.4f;
        [SerializeField] private float currentDistanceBehind = 42.0f;
        [SerializeField] private float chaseSmoothing = 2.5f;

        [Header("Level Multipliers")]
        [SerializeField] private float ghostAggression = 1.0f;

        private bool isCatching = false;
        private bool isChaseActive = false;
        private float whisperTimer = 0f;
        private float nextWhisperInterval = 8f;

        public float CurrentDistanceBehind => currentDistanceBehind;
        public bool IsDangerClose => currentDistanceBehind < 10.0f;
        public bool IsChaseActive => isChaseActive;

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
            if (tractor == null) tractor = FindAnyObjectByType<TractorController>();
            if (road == null) road = FindAnyObjectByType<ImportedRoadBuilder>();

            EnsureGhostExists();
            ResetChase();
        }

        public void EnsureGhostExists()
        {
            if (ghost == null)
            {
                ghost = FindAnyObjectByType<GhostController>(FindObjectsInactive.Include);
                if (ghost == null)
                {
                    GameObject ghostObj = GhostFaceImporter.BuildGhostFace();
                    ghost = ghostObj.GetComponent<GhostController>();
                }
            }

            if (ghost != null)
            {
                ghost.gameObject.SetActive(false);
            }
        }

        public void SetLevelAggression(int level)
        {
            // Level 1: 1.0x, Level 2: 1.15x, Level 3: 1.3x, Level 4: 1.45x, Level 5: 1.6x
            ghostAggression = 1.0f + (level - 1) * 0.15f;
            ResetChase();
        }

        public void ResetChase()
        {
            isCatching = false;
            isChaseActive = false;
            currentDistanceBehind = 42.0f;
            whisperTimer = 0f;

            if (ghost != null)
            {
                ghost.gameObject.SetActive(false);
            }
        }

        public void StartChase()
        {
            isChaseActive = true;
            isCatching = false;
            currentDistanceBehind = 42.0f;

            if (ghost != null)
            {
                ghost.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (!isChaseActive || isCatching || tractor == null) return;
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            float timeRemaining = GameManager.Instance.TimeRemaining;
            float elapsed = Mathf.Clamp(60.0f - timeRemaining, 0f, 60f);

            // 1. Calculate Base Target Distance according to 60-Second Challenge GDD progression
            float baseTargetDist;
            if (elapsed < 10f)
            {
                // 0–10s: Distant shadow & stealth build-up
                baseTargetDist = Mathf.Lerp(45f, 35f, elapsed / 10f);
            }
            else if (elapsed < 20f)
            {
                // 10–20s: Distant silhouette visible in rear view
                baseTargetDist = Mathf.Lerp(35f, 25f, (elapsed - 10f) / 10f);
            }
            else if (elapsed < 40f)
            {
                // 20–40s: Ghost becomes clearly visible
                baseTargetDist = Mathf.Lerp(25f, 15f, (elapsed - 20f) / 20f);
            }
            else if (elapsed < 55f)
            {
                // 40–55s: Ghost closes in aggressively
                baseTargetDist = Mathf.Lerp(15f, 8f, (elapsed - 40f) / 15f);
            }
            else
            {
                // 55–60s: High-intensity final chase!
                baseTargetDist = Mathf.Lerp(8f, 3.5f, (elapsed - 55f) / 5f);
            }

            // 2. Adjust for Player Driving Performance
            // Tractor speed: ~7 m/s (25 km/h) is good speed.
            float playerSpeed = tractor.CurrentSpeed;
            float speedModifier = 0f;

            if (playerSpeed > 6.0f)
            {
                // Driving fast & clean: push ghost back
                speedModifier = 4.5f;
            }
            else if (playerSpeed < 1.5f || tractor.IsInMud || !tractor.IsEngineRunning)
            {
                // Slowed down, stopped in mud, or crashed: ghost surges closer!
                speedModifier = -7.5f * ghostAggression;
            }

            float finalTargetDist = Mathf.Clamp(baseTargetDist + speedModifier, 1.8f, maxChaseDistance);

            // 3. Smooth Distance Interpolation
            float lerpSpeed = (speedModifier < 0f) ? (chaseSmoothing * 1.5f) : chaseSmoothing;
            currentDistanceBehind = Mathf.MoveTowards(currentDistanceBehind, finalTargetDist, Time.deltaTime * lerpSpeed * ghostAggression);

            // 4. Update Ghost Position & Rotation along road behind tractor
            UpdateGhostPosition();

            // 5. Environmental Audio & Whispers
            HandleGhostAtmosphere();

            // 6. Check Catch Condition (~2.4 meters)
            if (currentDistanceBehind <= catchDistance)
            {
                StartCoroutine(CatchPlayerSequence());
            }
        }

        private void UpdateGhostPosition()
        {
            if (ghost == null || tractor == null) return;

            Vector3 tractorPos = tractor.transform.position;
            Vector3 tractorFwd = tractor.transform.forward;

            // Ghost follows along road path / directly behind tractor
            Vector3 desiredGhostPos = tractorPos - tractorFwd * currentDistanceBehind + Vector3.up * 0.45f;

            // Add subtle floating sway & hovering bob
            float hoverSwayX = Mathf.Sin(Time.time * 2.8f) * 0.4f;
            float hoverSwayY = Mathf.Sin(Time.time * 3.5f) * 0.25f;
            desiredGhostPos += tractor.transform.right * hoverSwayX + Vector3.up * hoverSwayY;

            ghost.transform.position = Vector3.Lerp(ghost.transform.position, desiredGhostPos, Time.deltaTime * 6f);

            // Face towards the tractor driver
            Vector3 lookDir = (tractorPos + Vector3.up * 1.0f - ghost.transform.position).normalized;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                ghost.transform.rotation = Quaternion.Slerp(ghost.transform.rotation, Quaternion.LookRotation(lookDir, Vector3.up), Time.deltaTime * 8f);
            }

            // Adjust eyes intensity based on proximity
            if (ghost.eyeLightLeft != null && ghost.eyeLightRight != null)
            {
                float eyeIntensity = Mathf.Lerp(3.5f, 0.8f, currentDistanceBehind / 30f);
                ghost.eyeLightLeft.intensity = eyeIntensity;
                ghost.eyeLightRight.intensity = eyeIntensity;
            }
        }

        private void HandleGhostAtmosphere()
        {
            whisperTimer += Time.deltaTime;
            if (whisperTimer > nextWhisperInterval)
            {
                whisperTimer = 0f;
                nextWhisperInterval = Random.Range(6f, 14f) / ghostAggression;

                if (ghost != null && currentDistanceBehind < 30f)
                {
                    AudioManager.Instance?.PlayGhostWhisper(ghost.transform.position);
                }
            }

            // Increase heartbeat when ghost is close
            if (currentDistanceBehind < 12f)
            {
                float proximityNorm = 1f - (currentDistanceBehind / 12f);
                AudioManager.Instance?.SetHeartbeatIntensity(proximityNorm * 0.9f);
            }
        }

        private IEnumerator CatchPlayerSequence()
        {
            isCatching = true;
            isChaseActive = false;

            // 1. Cut tractor engine & trigger stall
            if (tractor != null)
            {
                tractor.StallEngine(5f);
            }

            // 2. Camera Trauma Shake & Jumpscare Flash
            var cam = FindAnyObjectByType<CameraController>();
            if (cam != null)
            {
                cam.AddTrauma(1.0f);
            }
            UIManager.Instance?.TriggerHorrorJumpscareFlash();

            // 3. Play terrifying jumpscare stinger
            AudioManager.Instance?.PlayJumpscare();

            // 4. Ghost lunges directly in front of camera / behind tractor driver
            if (ghost != null && tractor != null)
            {
                ghost.transform.position = tractor.transform.position - tractor.transform.forward * 1.2f + Vector3.up * 1.3f;
                ghost.transform.LookAt(tractor.transform.position + Vector3.up * 1.2f);
            }

            yield return new WaitForSeconds(1.1f);

            // 5. Trigger Game Over
            GameManager.Instance?.TriggerGameOver("The ghost caught you... Her cold hands pulled you into the darkness.");
        }
    }
}
