using UnityEngine;
using BhootiyaRasta.Audio;

namespace BhootiyaRasta.Vehicle
{
    [RequireComponent(typeof(TractorController))]
    public class TractorAudio : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource hornSource;
        [SerializeField] private AudioSource surfaceSource;

        [Header("Settings")]
        [SerializeField] private float minPitch = 0.75f;
        [SerializeField] private float maxPitch = 1.65f;
        [SerializeField] private float minVolume = 0.35f;
        [SerializeField] private float maxVolume = 0.85f;

        private TractorController controller;
        private AudioClip engineClip;
        private AudioClip hornClip;

        private void Awake()
        {
            controller = GetComponent<TractorController>();
            SetupSources();
        }

        private void SetupSources()
        {
            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.spatialBlend = 0.6f;
                engineSource.minDistance = 3f;
                engineSource.maxDistance = 35f;
                engineSource.loop = true;
            }

            if (hornSource == null)
            {
                hornSource = gameObject.AddComponent<AudioSource>();
                hornSource.spatialBlend = 0.7f;
                hornSource.minDistance = 5f;
                hornSource.maxDistance = 60f;
            }

            if (surfaceSource == null)
            {
                surfaceSource = gameObject.AddComponent<AudioSource>();
                surfaceSource.spatialBlend = 0.5f;
                surfaceSource.loop = true;
            }

            engineClip = ProceduralAudioClips.CreateTractorEngineLoop();
            hornClip = ProceduralAudioClips.CreateTractorHorn();

            engineSource.clip = engineClip;
            engineSource.Play();

            hornSource.clip = hornClip;
        }

        private void Update()
        {
            if (controller == null) return;

            if (controller.IsEngineRunning)
            {
                if (!engineSource.isPlaying) engineSource.Play();

                float speedFactor = Mathf.Clamp01(Mathf.Abs(controller.CurrentSpeed) / controller.MaxForwardSpeed);
                float throttleFactor = Mathf.Abs(controller.ThrottleInput);
                float combinedRpm = Mathf.Clamp01(speedFactor * 0.6f + throttleFactor * 0.4f);

                engineSource.pitch = Mathf.Lerp(engineSource.pitch, Mathf.Lerp(minPitch, maxPitch, combinedRpm), Time.deltaTime * 6f);
                engineSource.volume = Mathf.Lerp(engineSource.volume, Mathf.Lerp(minVolume, maxVolume, combinedRpm), Time.deltaTime * 8f);
            }
            else
            {
                // Engine died / stalled by ghost
                engineSource.pitch = Mathf.Lerp(engineSource.pitch, 0.4f, Time.deltaTime * 3f);
                engineSource.volume = Mathf.Lerp(engineSource.volume, 0f, Time.deltaTime * 4f);
                if (engineSource.volume < 0.05f) engineSource.Stop();
            }
        }

        public void PlayHorn()
        {
            if (hornSource != null && hornClip != null && !hornSource.isPlaying)
            {
                hornSource.Play();
            }
        }

        public void PlaySputterSound()
        {
            if (engineSource != null)
            {
                engineSource.pitch = 0.5f;
            }
        }
    }
}
