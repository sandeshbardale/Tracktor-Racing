using UnityEngine;

namespace BhootiyaRasta.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        [Header("Lighting")]
        public Light moonLight;
        public Color moonColor = new Color(0.18f, 0.24f, 0.38f);
        public Color ambientSkyColor = new Color(0.04f, 0.06f, 0.10f);

        [Header("Atmosphere")]
        public ParticleSystem firefliesSystem;
        public ParticleSystem mistSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ApplyLevelAtmosphere(int level)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 25f;
            RenderSettings.fogEndDistance = 240f;

            // Gradient Trilight Ambient Lighting for rich, realistic depth & indirect bounce
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.20f, 0.26f, 0.38f);       // Atmospheric night sky bounce
            RenderSettings.ambientEquatorColor = new Color(0.14f, 0.18f, 0.28f);   // Horizon bounce
            RenderSettings.ambientGroundColor = new Color(0.09f, 0.10f, 0.14f);    // Earth/soil ground bounce

            Color fogColor = new Color(0.12f, 0.17f, 0.27f); // Natural midnight mist hue
            float moonIntensity = 0.85f; // Crisp, clear moonlight
            Color moonHue = new Color(0.72f, 0.82f, 0.98f);  // Luminous natural cold white/blue moonlight

            switch (level)
            {
                case 1: // The Beginning - Clear Moonlit Night
                    RenderSettings.fogStartDistance = 35f;
                    RenderSettings.fogEndDistance = 260f;
                    fogColor = new Color(0.13f, 0.18f, 0.29f);
                    moonIntensity = 0.90f;
                    break;
                case 2: // The Empty Fields - Vast Night Horizons
                    RenderSettings.fogStartDistance = 30f;
                    RenderSettings.fogEndDistance = 240f;
                    fogColor = new Color(0.12f, 0.17f, 0.27f);
                    moonIntensity = 0.85f;
                    break;
                case 3: // The Abandoned Village - Atmospheric Eerie Haze
                    RenderSettings.fogStartDistance = 25f;
                    RenderSettings.fogEndDistance = 210f;
                    fogColor = new Color(0.11f, 0.16f, 0.25f);
                    moonIntensity = 0.80f;
                    break;
                case 4: // The Forest Road - Mysterious Canopy Mist
                    RenderSettings.fogStartDistance = 20f;
                    RenderSettings.fogEndDistance = 185f;
                    fogColor = new Color(0.10f, 0.15f, 0.24f);
                    moonIntensity = 0.75f;
                    break;
                case 5: // Bhootiya Rasta - Supernatural Dense Midnight
                    RenderSettings.fogStartDistance = 15f;
                    RenderSettings.fogEndDistance = 160f;
                    fogColor = new Color(0.09f, 0.14f, 0.22f);
                    moonIntensity = 0.70f;
                    break;
            }

            RenderSettings.fogColor = fogColor;

            if (moonLight != null)
            {
                moonLight.color = moonHue;
                moonLight.intensity = moonIntensity;
                moonLight.shadows = LightShadows.Soft;
                moonLight.shadowStrength = 0.85f;
            }

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = fogColor;
            }
        }
    }
}
