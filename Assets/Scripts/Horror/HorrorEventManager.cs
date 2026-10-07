using System.Collections;
using UnityEngine;
using BhootiyaRasta.Audio;

namespace BhootiyaRasta.Horror
{
    public class HorrorEventManager : MonoBehaviour
    {
        public static HorrorEventManager Instance { get; private set; }

        [Header("Environmental Horror")]
        [SerializeField] private float minAtmosphericEventInterval = 20f;
        [SerializeField] private float maxAtmosphericEventInterval = 40f;

        private float eventTimer = 0f;

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
            eventTimer = Random.Range(10f, 20f);
        }

        private void Update()
        {
            eventTimer -= Time.deltaTime;
            if (eventTimer <= 0f)
            {
                TriggerRandomEnvironmentalScare();
                eventTimer = Random.Range(minAtmosphericEventInterval, maxAtmosphericEventInterval);
            }
        }

        public void TriggerRandomEnvironmentalScare()
        {
            int r = Random.Range(0, 3);
            switch (r)
            {
                case 0:
                    // Sudden hush/silence
                    AudioManager.Instance?.TriggerSuddenSilence(3.5f);
                    break;
                case 1:
                    // Wind surge
                    AudioManager.Instance?.SetHeartbeatIntensity(0.4f);
                    StartCoroutine(ResetHeartbeatAfter(4f));
                    break;
                case 2:
                    // Distant whisper
                    if (Camera.main != null)
                    {
                        Vector3 randomDir = Random.onUnitSphere;
                        randomDir.y = 0.2f;
                        AudioManager.Instance?.PlayGhostWhisper(Camera.main.transform.position + randomDir * 15f);
                    }
                    break;
            }
        }

        private IEnumerator ResetHeartbeatAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            AudioManager.Instance?.SetHeartbeatIntensity(0f);
        }
    }
}
