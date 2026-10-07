using UnityEngine;

namespace BhootiyaRasta.Audio
{
    public static class ProceduralAudioClips
    {
        public static AudioClip CreateTractorEngineLoop()
        {
            int sampleRate = 44100;
            float duration = 1.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                // 3-cylinder diesel pulse around 28 Hz base idle
                float pulse1 = Mathf.Sin(2f * Mathf.PI * 28f * t);
                float pulse2 = Mathf.Sin(2f * Mathf.PI * 56f * t) * 0.6f;
                float pulse3 = Mathf.Sin(2f * Mathf.PI * 84f * t) * 0.3f;
                float thud = Mathf.Pow(Mathf.Abs(Mathf.Sin(2f * Mathf.PI * 14f * t)), 6f);
                float mechanicalChug = ((Random.value * 2f - 1f) * 0.15f) * thud;
                
                float sample = (pulse1 + pulse2 + pulse3) * 0.4f * thud + mechanicalChug;
                data[i] = Mathf.Clamp(sample, -0.9f, 0.9f);
            }

            AudioClip clip = AudioClip.Create("TractorEngineLoop", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateTractorHorn()
        {
            int sampleRate = 44100;
            float duration = 1.2f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f;
                if (t < 0.05f) env = t / 0.05f;
                else if (t > 1.0f) env = (duration - t) / 0.2f;

                // Indian dual-tone brassy horn (approx F4 & A4 = ~349Hz and 440Hz)
                float f1 = Mathf.Sin(2f * Mathf.PI * 349f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.8f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 698f * t) * 0.35f;

                data[i] = (f1 + f2 + f3) * 0.35f * env;
            }

            AudioClip clip = AudioClip.Create("TractorHorn", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateNightCrickets()
        {
            int sampleRate = 44100;
            float duration = 3.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                // cricket chirping chirp packets
                float chirpPulse = Mathf.Sin(2f * Mathf.PI * 18f * t);
                float isChirping = chirpPulse > 0.4f ? 1f : 0f;
                float carrier = Mathf.Sin(2f * Mathf.PI * 4600f * t) * 0.7f + Mathf.Sin(2f * Mathf.PI * 5200f * t) * 0.3f;
                float rustle = (Random.value * 2f - 1f) * 0.04f;

                data[i] = (carrier * isChirping * 0.12f) + rustle;
            }

            AudioClip clip = AudioClip.Create("NightCrickets", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEerieWind()
        {
            int sampleRate = 44100;
            float duration = 4.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];
            float lastVal = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float lfo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float white = Random.value * 2f - 1f;
                // Brown noise filter
                lastVal = (lastVal + (0.02f * white)) / 1.02f;
                float howl = Mathf.Sin(2f * Mathf.PI * (180f + 60f * lfo) * t) * 0.15f;

                data[i] = (lastVal * 2.5f + howl) * (0.25f + 0.2f * lfo);
            }

            AudioClip clip = AudioClip.Create("EerieWind", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateGhostWhisper()
        {
            int sampleRate = 44100;
            float duration = 2.5f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float lfo = Mathf.Sin(2f * Mathf.PI * 3f * t);
                float noise = (Random.value * 2f - 1f);
                float formant = Mathf.Sin(2f * Mathf.PI * (700f + 200f * lfo) * t) * 0.2f;
                float eerieHigh = Mathf.Sin(2f * Mathf.PI * 1850f * t) * 0.1f;

                data[i] = (noise * 0.15f + formant + eerieHigh) * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("GhostWhisper", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateJumpscareStinger()
        {
            int sampleRate = 44100;
            float duration = 1.8f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float decay = Mathf.Exp(-3.5f * t);
                // Sub impact
                float sub = Mathf.Sin(2f * Mathf.PI * 55f * Mathf.Exp(-1.5f * t) * t) * 0.7f;
                // Dissonant metallic screech
                float screech1 = Mathf.Sin(2f * Mathf.PI * (1150f + 300f * Mathf.Sin(60f * t)) * t);
                float screech2 = Mathf.Sin(2f * Mathf.PI * 1630f * t);
                float noiseHit = (Random.value * 2f - 1f) * 0.4f;

                float sample = (sub + (screech1 + screech2) * 0.35f + noiseHit) * decay;
                data[i] = Mathf.Clamp(sample, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("JumpscareStinger", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateHeartbeat()
        {
            int sampleRate = 44100;
            float duration = 0.8f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float lub = 0f;
                if (t < 0.25f)
                {
                    float lt = t / 0.25f;
                    lub = Mathf.Sin(2f * Mathf.PI * 45f * t) * Mathf.Sin(Mathf.PI * lt);
                }
                float dub = 0f;
                if (t >= 0.25f && t < 0.55f)
                {
                    float dt = (t - 0.25f) / 0.3f;
                    dub = Mathf.Sin(2f * Mathf.PI * 52f * (t - 0.25f)) * Mathf.Sin(Mathf.PI * dt) * 0.8f;
                }

                data[i] = (lub + dub) * 0.6f;
            }

            AudioClip clip = AudioClip.Create("Heartbeat", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateMudSquelch()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int totalSamples = (int)(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Sin(Mathf.PI * (t / duration));
                float noise = (Random.value * 2f - 1f) * 0.3f;
                float squelch = Mathf.Sin(2f * Mathf.PI * (250f - 180f * t) * t) * 0.5f;

                data[i] = (noise + squelch) * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create("MudSquelch", totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
