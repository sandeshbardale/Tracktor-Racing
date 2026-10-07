using System.Collections;
using UnityEngine;
using BhootiyaRasta.Vehicle;
using BhootiyaRasta.CameraControl;
using BhootiyaRasta.Audio;
using BhootiyaRasta.UI;

namespace BhootiyaRasta.Horror
{
    public class GhostEncounterManager : MonoBehaviour
    {
        public static GhostEncounterManager Instance { get; private set; }

        [Header("References")]
        public TractorController tractor;
        public CameraController cameraController;
        public GhostController ghostPrefab;

        [Header("Pacing Settings")]
        [SerializeField] private float minEventInterval = 14f;
        [SerializeField] private float maxEventInterval = 28f;
        [SerializeField] private float initialDelay = 10f;
        [SerializeField] private int currentLevel = 1;

        private GhostController activeGhost;
        private float nextEventTimer = 0f;
        private bool isEventActive = false;
        private bool hasTriggeredRearLookScare = false;

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
            if (cameraController == null) cameraController = FindAnyObjectByType<CameraController>();

            if (activeGhost == null)
            {
                if (ghostPrefab != null)
                {
                    activeGhost = Instantiate(ghostPrefab);
                }
                else
                {
                    activeGhost = FindAnyObjectByType<GhostController>(FindObjectsInactive.Include);
                    if (activeGhost == null)
                    {
                        GameObject gObj = GhostFaceImporter.BuildGhostFace();
                        activeGhost = gObj.GetComponent<GhostController>();
                    }
                }

                if (activeGhost != null)
                {
                    activeGhost.gameObject.SetActive(false);
                }
            }

            ConfigurePacingForLevel(currentLevel);
            nextEventTimer = initialDelay;
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
            ConfigurePacingForLevel(level);
        }

        private void ConfigurePacingForLevel(int level)
        {
            switch (level)
            {
                case 1: // The Beginning: gentle introduction, 1-2 scares
                    minEventInterval = 25f;
                    maxEventInterval = 45f;
                    break;
                case 2: // The Empty Fields: more frequent, strange field sounds
                    minEventInterval = 18f;
                    maxEventInterval = 32f;
                    break;
                case 3: // The Abandoned Village: village scares, flickering poles
                    minEventInterval = 14f;
                    maxEventInterval = 26f;
                    break;
                case 4: // The Forest Road: close scares, heavy tension
                    minEventInterval = 11f;
                    maxEventInterval = 20f;
                    break;
                case 5: // Bhootiya Rasta: intense, unpredictable, multiple scares
                    minEventInterval = 7f;
                    maxEventInterval = 15f;
                    break;
            }
        }

        private void Update()
        {
            if (tractor == null || isEventActive) return;

            // Check for rear-look scare opportunity (#5)
            if (cameraController != null && cameraController.IsLookingBack)
            {
                if (!hasTriggeredRearLookScare && Random.value < 0.25f && nextEventTimer <= 5f)
                {
                    hasTriggeredRearLookScare = true;
                    StartCoroutine(Event5_GhostBehindTractorOnLookBack());
                    return;
                }
            }

            nextEventTimer -= Time.deltaTime;
            if (nextEventTimer <= 0f)
            {
                TriggerRandomGhostEvent();
                nextEventTimer = Random.Range(minEventInterval, maxEventInterval);
                hasTriggeredRearLookScare = false;
            }
        }

        public void TriggerRandomGhostEvent()
        {
            if (isEventActive) return;

            // Choose an event based on level progression
            int maxEventIndex = 6;
            if (currentLevel >= 2) maxEventIndex = 8;
            if (currentLevel >= 3) maxEventIndex = 10;
            if (currentLevel >= 4) maxEventIndex = 12;

            int eventId = Random.Range(1, maxEventIndex + 1);

            switch (eventId)
            {
                case 1: StartCoroutine(Event1_MiddleOfRoad()); break;
                case 2: StartCoroutine(Event2_DistantVanishOnApproach()); break;
                case 3: StartCoroutine(Event3_BesideRoadStare()); break;
                case 4: StartCoroutine(Event4_CrossRoadQuickly()); break;
                case 5: StartCoroutine(Event5_GhostBehindTractorOnLookBack()); break;
                case 6: StartCoroutine(Event6_CloseTractorJumpscare()); break;
                case 7: StartCoroutine(Event7_RoadShadowGlides()); break;
                case 8: StartCoroutine(Event8_DistantFigureWalksTowardTractor()); break;
                case 9: StartCoroutine(Event9_GhostNearAncientTree()); break;
                case 10: StartCoroutine(Event10_HeadlightsFlickerScare()); break;
                case 11: StartCoroutine(Event11_EngineStallInDeadSilence()); break;
                case 12: StartCoroutine(Event12_MysteriousFieldWhispers()); break;
            }
        }

        // EVENT 1: Ghost suddenly appears in middle of the road
        private IEnumerator Event1_MiddleOfRoad()
        {
            isEventActive = true;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 24f;
            Quaternion spawnRot = Quaternion.LookRotation(-tractor.transform.forward);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.4f);
            AudioManager.Instance?.SetHeartbeatIntensity(0.7f);

            // Wait until tractor is close or time passes
            float timer = 0f;
            while (timer < 4f)
            {
                timer += Time.deltaTime;
                if (Vector3.Distance(tractor.transform.position, activeGhost.transform.position) < 8f)
                {
                    break;
                }
                yield return null;
            }

            activeGhost.Vanish(0.3f);
            AudioManager.Instance?.SetHeartbeatIntensity(0f);
            isEventActive = false;
        }

        // EVENT 2: Ghost stands far away and vanishes when headlights reach it
        private IEnumerator Event2_DistantVanishOnApproach()
        {
            isEventActive = true;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 45f;
            Quaternion spawnRot = Quaternion.LookRotation(-tractor.transform.forward);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.6f);

            float timer = 0f;
            while (timer < 6f)
            {
                timer += Time.deltaTime;
                if (Vector3.Distance(tractor.transform.position, activeGhost.transform.position) < 22f)
                {
                    // Headlights reach it -> sudden vanish
                    break;
                }
                yield return null;
            }

            activeGhost.Vanish(0.2f);
            isEventActive = false;
        }

        // EVENT 3: Ghost appears beside the road watching
        private IEnumerator Event3_BesideRoadStare()
        {
            isEventActive = true;
            float side = Random.value > 0.5f ? 1f : -1f;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 20f + tractor.transform.right * (side * 4.5f);
            Quaternion spawnRot = Quaternion.LookRotation(tractor.transform.position - spawnPos);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.5f);

            yield return new WaitForSeconds(3.5f);
            activeGhost.Vanish(0.4f);
            isEventActive = false;
        }

        // EVENT 4: Ghost crosses the road rapidly from field to field
        private IEnumerator Event4_CrossRoadQuickly()
        {
            isEventActive = true;
            float side = Random.value > 0.5f ? 1f : -1f;
            Vector3 startPos = tractor.transform.position + tractor.transform.forward * 18f + tractor.transform.right * (side * 8f);
            Vector3 endPos = startPos - tractor.transform.right * (side * 16f);

            EnsureGhostExists();
            activeGhost.GlideAcross(startPos, endPos, 14f);
            AudioManager.Instance?.PlayGhostWhisper(startPos);

            yield return new WaitForSeconds(2.0f);
            isEventActive = false;
        }

        // EVENT 5: Ghost appears behind the tractor when the player looks back
        private IEnumerator Event5_GhostBehindTractorOnLookBack()
        {
            isEventActive = true;
            Vector3 spawnPos = tractor.transform.position - tractor.transform.forward * 3.5f + Vector3.up * 0.4f;
            Quaternion spawnRot = Quaternion.LookRotation(tractor.transform.forward);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.1f);
            cameraController?.AddTrauma(0.7f);
            AudioManager.Instance?.PlayJumpscare();
            UIManager.Instance?.TriggerHorrorJumpscareFlash();

            yield return new WaitForSeconds(1.8f);
            activeGhost.Vanish(0.2f);
            isEventActive = false;
        }

        // EVENT 6: Ghost suddenly appears close to the tractor (Hood jumpscare)
        private IEnumerator Event6_CloseTractorJumpscare()
        {
            isEventActive = true;
            AudioManager.Instance?.TriggerSuddenSilence(1.0f);
            yield return new WaitForSeconds(1.0f);

            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 3.8f + Vector3.up * 0.3f;
            Quaternion spawnRot = Quaternion.LookRotation(-tractor.transform.forward);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.05f);
            cameraController?.AddTrauma(0.9f);
            AudioManager.Instance?.PlayJumpscare();
            UIManager.Instance?.TriggerHorrorJumpscareFlash();

            yield return new WaitForSeconds(1.5f);
            activeGhost.Vanish(0.15f);
            isEventActive = false;
        }

        // EVENT 7: A shadow appears on the road but disappears
        private IEnumerator Event7_RoadShadowGlides()
        {
            isEventActive = true;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 22f;
            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, Quaternion.identity, 1.2f);

            yield return new WaitForSeconds(2.0f);
            activeGhost.Vanish(0.8f);
            isEventActive = false;
        }

        // EVENT 8: Distant human figure slowly walks toward the tractor and vanishes
        private IEnumerator Event8_DistantFigureWalksTowardTractor()
        {
            isEventActive = true;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 38f;
            Quaternion spawnRot = Quaternion.LookRotation(-tractor.transform.forward);

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, spawnRot, 0.5f);
            activeGhost.ApproachTractor(tractor.transform, 3.5f);
            AudioManager.Instance?.SetHeartbeatIntensity(0.8f);

            yield return new WaitForSeconds(3.5f);
            AudioManager.Instance?.SetHeartbeatIntensity(0f);
            isEventActive = false;
        }

        // EVENT 9: Ghost appears near a tree / roadside structure
        private IEnumerator Event9_GhostNearAncientTree()
        {
            isEventActive = true;
            float side = Random.value > 0.5f ? 1f : -1f;
            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 22f + tractor.transform.right * (side * 5f) + Vector3.up * 1.5f;

            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, Quaternion.LookRotation(tractor.transform.position - spawnPos), 0.8f);

            yield return new WaitForSeconds(3.0f);
            activeGhost.Vanish(0.4f);
            isEventActive = false;
        }

        // EVENT 10: Headlights flicker wildly and ghost looms
        private IEnumerator Event10_HeadlightsFlickerScare()
        {
            isEventActive = true;
            tractor.TriggerHeadlightFlicker(3.5f);

            yield return new WaitForSeconds(1.0f);

            Vector3 spawnPos = tractor.transform.position + tractor.transform.forward * 10f;
            EnsureGhostExists();
            activeGhost.SpawnAndFadeIn(spawnPos, Quaternion.LookRotation(-tractor.transform.forward), 0.2f);

            yield return new WaitForSeconds(2.0f);
            activeGhost.Vanish(0.25f);
            isEventActive = false;
        }

        // EVENT 11: Engine suddenly stops and restarts after silence
        private IEnumerator Event11_EngineStallInDeadSilence()
        {
            isEventActive = true;
            AudioManager.Instance?.TriggerSuddenSilence(4.0f);
            tractor.StallEngine(3.8f);
            cameraController?.AddTrauma(0.35f);

            yield return new WaitForSeconds(2.0f);
            AudioManager.Instance?.PlayGhostWhisper(tractor.transform.position + tractor.transform.right * 2f);

            yield return new WaitForSeconds(2.0f);
            isEventActive = false;
        }

        // EVENT 12: Mysterious sound comes from the fields
        private IEnumerator Event12_MysteriousFieldWhispers()
        {
            isEventActive = true;
            float side = Random.value > 0.5f ? 1f : -1f;
            Vector3 fieldPos = tractor.transform.position + tractor.transform.right * (side * 8f) + tractor.transform.forward * 5f;

            AudioManager.Instance?.PlayGhostWhisper(fieldPos);
            AudioManager.Instance?.SetHeartbeatIntensity(0.5f);

            yield return new WaitForSeconds(3.0f);
            AudioManager.Instance?.SetHeartbeatIntensity(0f);
            isEventActive = false;
        }

        private void EnsureGhostExists()
        {
            if (activeGhost == null)
            {
                if (ghostPrefab != null)
                {
                    activeGhost = Instantiate(ghostPrefab);
                }
                else
                {
                    GameObject go = new GameObject("Ghost_Runtime");
                    activeGhost = go.AddComponent<GhostController>();
                }
            }
        }
    }
}
