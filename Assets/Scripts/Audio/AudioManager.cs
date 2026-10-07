using UnityEngine;
using UnityEngine.SceneManagement;

namespace TractorRacing
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Clips")]
        public AudioClip engineLoopClip;
        public AudioClip hornClip;
        public AudioClip countdownBeepClip;
        public AudioClip countdownGoClip;
        public AudioClip checkpointClip;
        public AudioClip levelCompleteClip;
        public AudioClip buttonClickClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            AudioListener.volume = 1f;
            AudioListener.pause = false;

            SetupSources();
            EnsureAudioListener();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();

            if (scene.name.StartsWith("Level") && scene.name != "LevelSelect")
            {
                if (engineSource != null && !engineSource.isPlaying)
                {
                    engineSource.Play();
                }
            }
            else
            {
                if (engineSource != null && engineSource.isPlaying)
                {
                    engineSource.Stop();
                }
            }
        }

        public void EnsureAudioListener()
        {
            if (FindObjectOfType<AudioListener>() == null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    GameObject listenerObj = new GameObject("AudioListener_Auto");
                    listenerObj.AddComponent<AudioListener>();
                }
            }
        }

        private void SetupSources()
        {
            if (engineSource == null) engineSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

            sfxSource.playOnAwake = false;
            engineSource.playOnAwake = false;
            musicSource.playOnAwake = false;

            // Generate procedural clips if none assigned
            if (countdownBeepClip == null) countdownBeepClip = CreateToneClip(440f, 0.2f);
            if (countdownGoClip == null) countdownGoClip = CreateToneClip(880f, 0.45f);
            if (checkpointClip == null) checkpointClip = CreateChimeClip();
            if (levelCompleteClip == null) levelCompleteClip = CreateFanfareClip();
            if (buttonClickClip == null) buttonClickClip = CreateToneClip(600f, 0.08f);
            if (hornClip == null) hornClip = CreateHornClip();
            if (engineLoopClip == null) engineLoopClip = CreateEngineHumClip();

            // Setup engine loop
            engineSource.clip = engineLoopClip;
            engineSource.loop = true;
            engineSource.volume = 0.5f;
            engineSource.pitch = 0.9f;

            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.StartsWith("Level") && sceneName != "LevelSelect")
            {
                engineSource.Play();
            }
        }

        public void SetEnginePitch(float speedRatio)
        {
            if (engineSource != null)
            {
                engineSource.pitch = Mathf.Lerp(0.85f, 1.85f, Mathf.Clamp01(speedRatio));
                engineSource.volume = Mathf.Lerp(0.4f, 0.85f, Mathf.Clamp01(speedRatio));
            }
        }

        public void PlayHorn()
        {
            if (sfxSource != null && hornClip != null)
            {
                sfxSource.PlayOneShot(hornClip, 1.0f);
            }
        }

        public void PlayCountdownBeep(bool isGo)
        {
            if (sfxSource == null) return;
            sfxSource.PlayOneShot(isGo ? countdownGoClip : countdownBeepClip, 0.95f);
        }

        public void PlayCheckpointPassed()
        {
            if (sfxSource != null && checkpointClip != null)
            {
                sfxSource.PlayOneShot(checkpointClip, 0.9f);
            }
        }

        public void PlayLevelComplete()
        {
            if (sfxSource != null && levelCompleteClip != null)
            {
                sfxSource.PlayOneShot(levelCompleteClip, 1.0f);
            }
        }

        public void PlayButtonClick()
        {
            if (sfxSource != null && buttonClickClip != null)
            {
                sfxSource.PlayOneShot(buttonClickClip, 0.8f);
            }
        }

        private static AudioClip CreateToneClip(float frequency, float duration)
        {
            int sampleRate = 44100;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Clamp01(1f - (t / duration));
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.6f;
            }

            AudioClip clip = AudioClip.Create($"Tone_{frequency}Hz", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateHornClip()
        {
            int sampleRate = 44100;
            float duration = 0.6f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = (t < 0.05f) ? (t / 0.05f) : ((t > 0.45f) ? (1f - (t - 0.45f) / 0.15f) : 1f);
                float wave = Mathf.Sin(2f * Mathf.PI * 340f * t) * 0.5f +
                             Mathf.Sin(2f * Mathf.PI * 420f * t) * 0.4f +
                             Mathf.Sin(2f * Mathf.PI * 680f * t) * 0.2f;
                samples[i] = wave * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("Tractor_Horn", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateChimeClip()
        {
            int sampleRate = 44100;
            float duration = 0.5f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-t * 7f);
                float wave = Mathf.Sin(2f * Mathf.PI * 587.33f * t) * 0.5f +
                             Mathf.Sin(2f * Mathf.PI * 880.00f * t) * 0.35f +
                             Mathf.Sin(2f * Mathf.PI * 1174.66f * t) * 0.2f;
                samples[i] = wave * env * 0.65f;
            }

            AudioClip clip = AudioClip.Create("Chime_Checkpoint", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateFanfareClip()
        {
            int sampleRate = 44100;
            float duration = 1.5f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float freq = (t < 0.25f) ? 523.25f : ((t < 0.5f) ? 659.25f : ((t < 0.75f) ? 783.99f : 1046.50f));
                float env = Mathf.Clamp01(1f - (t / duration));
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                samples[i] = wave * env * 0.55f;
            }

            AudioClip clip = AudioClip.Create("Fanfare_Win", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateEngineHumClip()
        {
            int sampleRate = 44100;
            float duration = 1.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float wave1 = Mathf.Sin(2f * Mathf.PI * 65f * t);
                float wave2 = Mathf.Sin(2f * Mathf.PI * 130f * t) * 0.6f;
                float wave3 = Mathf.Sin(2f * Mathf.PI * 195f * t) * 0.3f;
                float chug = Mathf.Sin(2f * Mathf.PI * 16.25f * t) * 0.25f;
                float noise = (Random.value * 2f - 1f) * 0.1f;
                samples[i] = (wave1 + wave2 + wave3 + chug + noise) * 0.45f;
            }

            AudioClip clip = AudioClip.Create("Engine_DieselHum", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

namespace BhootiyaRasta.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void PlayJumpscare() {}
        public void PlayGhostWhisper(Vector3 position) {}
        public void PlayMudSquelch(Vector3 position) {}
        public void SetHeartbeatIntensity(float intensity) {}
        public void TriggerSuddenSilence(float duration = 4.0f) {}
    }
}
