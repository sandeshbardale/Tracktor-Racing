using UnityEngine;
using BhootiyaRasta.Vehicle;
using BhootiyaRasta.CameraControl;
using BhootiyaRasta.Horror;
using BhootiyaRasta.Environment;
using BhootiyaRasta.Audio;
using BhootiyaRasta.UI;

namespace BhootiyaRasta.Core
{
    public class BhootiyaGameBootstrapper : MonoBehaviour
    {
        // Bhootiya Rasta bootstrapper disabled in favor of pure Tractor Racing
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void EnsureSystemsExist()
        {
            if (FindAnyObjectByType<GameManager>() != null) return;

            Debug.Log("<color=#FFA500>[Bhootiya Rasta] Initializing Realistic Indian Rural Horror World...</color>");

            // 1. Generate PBR Procedural Textures
            Texture2D roadTex = TextureGenerator.CreateDirtRoadTexture(512, 512);
            Texture2D roadNormal = TextureGenerator.CreateRoadNormalMap(512, 512);
            Texture2D farmSoilTex = TextureGenerator.CreateFarmlandSoilTexture(256, 256);
            Texture2D tractorPaintTex = TextureGenerator.CreateTractorPaintTexture(256, 256);
            Texture2D tireRubberTex = TextureGenerator.CreateTireRubberTexture(256, 256);
            Texture2D clayWallTex = TextureGenerator.CreateClayMudWallTexture(256, 256);
            Texture2D thatchRoofTex = TextureGenerator.CreateThatchStrawTexture(256, 256);
            Texture2D woodTex = TextureGenerator.CreateWeatheredWoodTexture(256, 256);
            Texture2D stoneTex = TextureGenerator.CreateAncientStoneTexture(256, 256);
            Texture2D ghostClothTex = TextureGenerator.CreateGhostClothTexture(256, 256);

            // 2. Realistic Materials
            Material tractorRedMat = CreateRuntimeTexturedMaterial("Mat_Tractor_Red", tractorPaintTex, 0.35f, 0.55f);
            Material tractorMetalMat = CreateRuntimeStandardMaterial("Mat_Tractor_Metal", new Color(0.24f, 0.25f, 0.28f), 0.75f, 0.45f);
            Material tireRubberMat = CreateRuntimeTexturedMaterial("Mat_Tractor_TireRubber", tireRubberTex, 0.0f, 0.15f);
            Material rimSteelMat = CreateRuntimeStandardMaterial("Mat_Tractor_RimSteel", new Color(0.85f, 0.82f, 0.45f), 0.5f, 0.4f);
            Material glassMat = CreateRuntimeStandardMaterial("Mat_Tractor_Glass", new Color(0.9f, 0.95f, 1f, 0.6f), 0.1f, 0.95f);
            Material headlightGlowMat = CreateRuntimeEmissiveMaterial("Mat_Headlight_Glow", new Color(1.0f, 0.92f, 0.75f));
            Material taillightGlowMat = CreateRuntimeEmissiveMaterial("Mat_Taillight_Glow", new Color(0.95f, 0.05f, 0.05f));

            Material roadDirtMat = CreateRuntimeTexturedMaterial("Mat_KacchaRoad_Dirt", roadTex, 0.0f, 0.15f, roadNormal);
            Material fieldGroundMat = CreateRuntimeTexturedMaterial("Mat_Rural_Fields", farmSoilTex, 0.0f, 0.05f);
            Material woodMat = CreateRuntimeTexturedMaterial("Mat_Rural_Wood", woodTex, 0.0f, 0.1f);
            Material thatchMat = CreateRuntimeTexturedMaterial("Mat_Rural_Thatch", thatchRoofTex, 0.0f, 0.05f);
            Material clayWallMat = CreateRuntimeTexturedMaterial("Mat_Rural_ClayWall", clayWallTex, 0.0f, 0.05f);
            Material stoneMat = CreateRuntimeTexturedMaterial("Mat_Rural_Stone", stoneTex, 0.0f, 0.1f);
            Material leafMat = CreateRuntimeStandardMaterial("Mat_Rural_Foliage", new Color(0.12f, 0.24f, 0.12f), 0.0f, 0.05f);
            Material cropMat = CreateRuntimeStandardMaterial("Mat_Rural_Crops", new Color(0.18f, 0.32f, 0.12f), 0.0f, 0.05f);
            Material mudMat = CreateRuntimeStandardMaterial("Mat_Mud_Puddle", new Color(0.12f, 0.08f, 0.05f), 0.2f, 0.92f);
            Material wireMat = CreateRuntimeStandardMaterial("Mat_Electric_Wire", new Color(0.08f, 0.08f, 0.09f), 0.6f, 0.2f);

            Material ghostSkinMat = CreateRuntimeStandardMaterial("Mat_Ghost_Skin", new Color(0.82f, 0.86f, 0.92f), 0.0f, 0.1f);
            Material ghostClothMat = CreateRuntimeTexturedMaterial("Mat_Ghost_Cloth", ghostClothTex, 0.0f, 0.05f);
            Material ghostHairMat = CreateRuntimeStandardMaterial("Mat_Ghost_Hair", new Color(0.02f, 0.02f, 0.03f), 0.0f, 0.05f);
            Material eyeGlowMat = CreateRuntimeEmissiveMaterial("Mat_Ghost_EyeGlow", new Color(0.95f, 0.12f, 0.12f));

            // Clean previous objects if any
            CleanExistingSceneObjects();

            // 3. Realistic Indian Tractor with 4 Rotating Tyres
            GameObject tractorObj = IndianTractorImporter.BuildIndianTractor();

            // 4. Realistic Ghost Face (Bhoot / Chudail)
            GameObject ghostObj = GhostFaceImporter.BuildGhostFace();
            ghostObj.SetActive(false);

            // 5. 1 AM Rural Night Directional Moonlight
            Light moonLight = SetupRuntimeMoonlight();

            // 6. Camera with realistic medium-distance follow
            SetupRuntimeCamera(tractorObj.transform);

            // 7. Environment, Imported Road & Obstacles
            GameObject envManagerObj = new GameObject("EnvironmentManager");
            var envMgr = envManagerObj.AddComponent<EnvironmentManager>();
            envMgr.moonLight = moonLight;

            GameObject roadObj = new GameObject("ImportedRoad");
            var importedRoad = roadObj.AddComponent<ImportedRoadBuilder>();
            importedRoad.BuildImportedRoadEnvironment();

            GameObject propObj = new GameObject("RuralPropGenerator");
            var propGen = propObj.AddComponent<RuralPropGenerator>();
            propGen.clayWallMat = clayWallMat;
            propGen.thatchRoofMat = thatchMat;
            propGen.weatheredWoodMat = woodMat;
            propGen.stoneMasonryMat = stoneMat;
            propGen.foliageLeafMat = leafMat;
            propGen.cropLeafMat = cropMat;
            propGen.rustedMetalMat = tractorMetalMat;
            propGen.electricalWireMat = wireMat;

            GameObject obsObj = new GameObject("RuralObstacleGenerator");
            var obsGen = obsObj.AddComponent<RuralObstacleGenerator>();
            obsGen.weatheredWoodMat = woodMat;
            obsGen.stoneMasonryMat = stoneMat;
            obsGen.mudPuddleMat = mudMat;
            obsGen.thatchStrawMat = thatchMat;
            obsGen.rustedMetalMat = tractorMetalMat;

            // 8. Audio & Ghost Chase System
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();

            GameObject chaseObj = new GameObject("GhostChaseManager");
            var ghostChase = chaseObj.AddComponent<GhostChaseManager>();
            ghostChase.tractor = tractorObj.GetComponent<TractorController>();
            ghostChase.ghost = ghostObj.GetComponent<GhostController>();
            ghostChase.road = importedRoad;

            GameObject horrorObj = new GameObject("GhostEncounterManager");
            var ghostMgr = horrorObj.AddComponent<GhostEncounterManager>();
            ghostMgr.tractor = tractorObj.GetComponent<TractorController>();
            ghostMgr.ghostPrefab = ghostObj.GetComponent<GhostController>();

            GameObject horrorEventObj = new GameObject("HorrorEventManager");
            horrorEventObj.AddComponent<HorrorEventManager>();

            // 9. Core & UI
            GameObject checkObj = new GameObject("CheckpointManager");
            var checkMgr = checkObj.AddComponent<CheckpointManager>();
            checkMgr.tractor = tractorObj.GetComponent<TractorController>();

            GameObject lvlObj = new GameObject("LevelManager");
            var lvlMgr = lvlObj.AddComponent<LevelManager>();
            lvlMgr.importedRoad = importedRoad;
            lvlMgr.propGenerator = propGen;
            lvlMgr.obstacleGenerator = obsGen;
            lvlMgr.environmentManager = envMgr;
            lvlMgr.ghostChaseManager = ghostChase;
            lvlMgr.tractor = tractorObj.GetComponent<TractorController>();

            GameObject uiObj = new GameObject("UIManager");
            uiObj.AddComponent<UIManager>();

            GameObject gameMgrObj = new GameObject("GameManager");
            gameMgrObj.AddComponent<GameManager>();

            // Load Level 1 into the world (Starts 60s countdown and intro)
            lvlMgr.LoadLevel(1);
        }

        private static void CleanExistingSceneObjects()
        {
            string[] toDelete = new string[] {
                "IndianTractor_Bhootni",
                "IndianTractor_Bhootiya",
                "IndianTractor_Mahindra",
                "Tractor_Racing",
                "Tractor_Old",
                "TractorBody_Mesh",
                "EngineHood_Mesh",
                "ChassisVisual",
                "IndianGhost_Bhootni",
                "IndianGhost_Chudail",
                "ImportedRoad",
                "ProceduralRoad",
                "RuralPropGenerator",
                "RuralObstacleGenerator",
                "EnvironmentManager",
                "AudioManager",
                "GhostChaseManager",
                "GhostEncounterManager",
                "HorrorEventManager",
                "CheckpointManager",
                "LevelManager",
                "UIManager",
                "GameManager"
            };

            foreach (var name in toDelete)
            {
                var objs = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var go in objs)
                {
                    if (go != null && go.name == name) Destroy(go);
                }
            }
        }

        private static Material CreateRuntimeTexturedMaterial(string name, Texture2D mainTex, float metallic, float smoothness, Texture2D normalMap = null)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            Material m = new Material(s);
            m.name = name;
            m.mainTexture = mainTex;
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", mainTex);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);

            if (normalMap != null && m.HasProperty("_BumpMap"))
            {
                m.EnableKeyword("_NORMALMAP");
                m.SetTexture("_BumpMap", normalMap);
            }
            return m;
        }

        private static Material CreateRuntimeStandardMaterial(string name, Color color, float metallic, float smoothness)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            Material m = new Material(s);
            m.name = name;
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        private static Material CreateRuntimeEmissiveMaterial(string name, Color color)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            Material m = new Material(s);
            m.name = name;
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_EmissionColor"))
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 2.8f);
            }
            return m;
        }

        private static Light SetupRuntimeMoonlight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null) lightObj = new GameObject("Directional Light");

            Light lt = lightObj.GetComponent<Light>();
            if (lt == null) lt = lightObj.AddComponent<Light>();

            lt.type = LightType.Directional;
            lt.color = new Color(0.72f, 0.82f, 0.98f);
            lt.intensity = 0.85f;
            lt.shadows = LightShadows.Soft;
            lt.shadowStrength = 0.85f;

            lightObj.transform.rotation = Quaternion.Euler(48f, -38f, 0f);
            return lt;
        }

        private static void SetupRuntimeCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.17f, 0.27f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.15f;
            cam.farClipPlane = 260f;

            var cc = cam.GetComponent<CameraController>();
            if (cc == null) cc = cam.gameObject.AddComponent<CameraController>();
            cc.SetTarget(target);
        }
    }
}
